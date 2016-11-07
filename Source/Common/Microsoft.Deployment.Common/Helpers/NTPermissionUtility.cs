using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Deployment.Common.Helpers
{
    public class NTPermissionUtility
    {
        public const string LOGON_AS_BATCH_PERM = "SeBatchLogonRight";

        private enum SID_NAME_USE
        {
            SidTypeUser = 1,
            SidTypeGroup,
            SidTypeDomain,
            SidTypeAlias,
            SidTypeWellKnownGroup,
            SidTypeDeletedAccount,
            SidTypeInvalid,
            SidTypeUnknown,
            SidTypeComputer
        }

        [DllImport("advapi32.dll", PreserveSig = true)]
        private static extern uint LsaOpenPolicy(ref LSA_UNICODE_STRING SystemName, ref LSA_OBJECT_ATTRIBUTES ObjectAttributes, int DesiredAccess, out IntPtr PolicyHandle);

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaAddAccountRights(IntPtr PolicyHandle, IntPtr AccountSid, LSA_UNICODE_STRING[] UserRights, int CountOfRights);

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaRemoveAccountRights(IntPtr PolicyHandle, IntPtr AccountSid, [MarshalAs(UnmanagedType.U1)] bool AllRights, // true will remove all rights
                                                          LSA_UNICODE_STRING[] UserRights,
                                                          uint CountOfRights);

        [DllImport("advapi32")]
        private static extern void FreeSid(IntPtr pSid);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, PreserveSig = true)]
        private static extern bool LookupAccountName(string lpSystemName, string lpAccountName, IntPtr Sid, ref uint cbSid, StringBuilder ReferencedDomainName,  ref uint cchReferencedDomainName, out SID_NAME_USE peUse);

        [DllImport("advapi32.dll")]
        private static extern bool IsValidSid(IntPtr pSid);

        [DllImport("advapi32.dll")]
        private static extern int LsaClose(IntPtr ObjectHandle);


        [DllImport("advapi32.dll")]
        private static extern int LsaNtStatusToWinError(uint status);

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaEnumerateAccountRights(IntPtr PolicyHandle, IntPtr AccountSid, out IntPtr UserRightsPtr, out int CountOfRights);

        [StructLayout(LayoutKind.Sequential)]
        private struct LSA_UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;

            public void SetTo(string s)
            {
                Clean();
                Buffer = Marshal.StringToHGlobalUni(s);
                Length = (ushort)(s.Length * UnicodeEncoding.CharSize);
                MaximumLength = (ushort)(Length + UnicodeEncoding.CharSize); // Accommodate an 0 at the end
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(Buffer, Length / UnicodeEncoding.CharSize);
            }

            public void Clean()
            {
                if (Buffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(Buffer);
                Buffer = IntPtr.Zero;
                Length = 0;
                MaximumLength = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LSA_OBJECT_ATTRIBUTES
        {
            public int Length;
            public IntPtr RootDirectory;
            public LSA_UNICODE_STRING ObjectName;
            public uint Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;
        }

        private static LSA_OBJECT_ATTRIBUTES CreateLSAObject()
        {
            LSA_OBJECT_ATTRIBUTES newInstance = new LSA_OBJECT_ATTRIBUTES();

            newInstance.Length = 0;
            newInstance.RootDirectory = IntPtr.Zero;
            newInstance.Attributes = 0;
            newInstance.SecurityDescriptor = IntPtr.Zero;
            newInstance.SecurityQualityOfService = IntPtr.Zero;

            return newInstance;
        }

        [Flags]
        private enum LSA_AccessPolicy : long
        {
            POLICY_VIEW_LOCAL_INFORMATION = 0x00000001L,
            POLICY_VIEW_AUDIT_INFORMATION = 0x00000002L,
            POLICY_GET_PRIVATE_INFORMATION = 0x00000004L,
            POLICY_TRUST_ADMIN = 0x00000008L,
            POLICY_CREATE_ACCOUNT = 0x00000010L,
            POLICY_CREATE_SECRET = 0x00000020L,
            POLICY_CREATE_PRIVILEGE = 0x00000040L,
            POLICY_SET_DEFAULT_QUOTA_LIMITS = 0x00000080L,
            POLICY_SET_AUDIT_REQUIREMENTS = 0x00000100L,
            POLICY_AUDIT_LOG_ADMIN = 0x00000200L,
            POLICY_SERVER_ADMIN = 0x00000400L,
            POLICY_LOOKUP_NAMES = 0x00000800L,
            POLICY_NOTIFICATION = 0x00001000L
        }

        private const int NO_ERROR = 0;
        private const int ERROR_INSUFFICIENT_BUFFER = 122;
        private const int ERROR_INVALID_FLAGS = 1004;

        public static void SetRight(string ComputerName, string domainAccount, string privilegeName, bool bRemove)
        {
            StringBuilder domainName = new StringBuilder(20);
            IntPtr        pSid       = IntPtr.Zero;
            uint          sidSize    = 0,
                          nameSize   = 0;
            SID_NAME_USE  accountType;
            int           winErrorCode;
            string        errorMessage;

            // This first call makes sure we get the buffer sizes correctly. We use NULL for system name, because it needs to be sent to the domains trusted by local system
            if (!LookupAccountName(null, domainAccount, pSid, ref sidSize, domainName, ref nameSize, out accountType))
            {
                winErrorCode = Marshal.GetLastWin32Error();
                if (winErrorCode == ERROR_INSUFFICIENT_BUFFER || winErrorCode == ERROR_INVALID_FLAGS)
                {
                    domainName.EnsureCapacity((int)nameSize);
                    pSid = Marshal.AllocHGlobal((int)sidSize);

                    if (!LookupAccountName(null, domainAccount, pSid, ref sidSize, domainName, ref nameSize, out accountType))
                    {
                        // Got the sizes corretly but other bad things happened.
                        winErrorCode = Marshal.GetLastWin32Error();
                        errorMessage = string.Format("LookupAccountName failed: {0}", winErrorCode);
                        throw new Win32Exception(winErrorCode, errorMessage);
                    }
                }
            }


            LSA_UNICODE_STRING systemName = new LSA_UNICODE_STRING();
            systemName.SetTo(ComputerName);

            IntPtr policyHandle = IntPtr.Zero;
            LSA_OBJECT_ATTRIBUTES objectAttributes = CreateLSAObject();

            // We are asking for too many permissions here - need to tone it down
            int desiredAccess = (int) ( LSA_AccessPolicy.POLICY_AUDIT_LOG_ADMIN |
                                        LSA_AccessPolicy.POLICY_CREATE_ACCOUNT |
                                        LSA_AccessPolicy.POLICY_CREATE_PRIVILEGE |
                                        LSA_AccessPolicy.POLICY_CREATE_SECRET |
                                        LSA_AccessPolicy.POLICY_GET_PRIVATE_INFORMATION |
                                        LSA_AccessPolicy.POLICY_LOOKUP_NAMES |
                                        LSA_AccessPolicy.POLICY_NOTIFICATION |
                                        LSA_AccessPolicy.POLICY_SERVER_ADMIN |
                                        LSA_AccessPolicy.POLICY_SET_AUDIT_REQUIREMENTS |
                                        LSA_AccessPolicy.POLICY_SET_DEFAULT_QUOTA_LIMITS |
                                        LSA_AccessPolicy.POLICY_TRUST_ADMIN |
                                        LSA_AccessPolicy.POLICY_VIEW_AUDIT_INFORMATION |
                                        LSA_AccessPolicy.POLICY_VIEW_LOCAL_INFORMATION
                                      );
            uint resultPolicy = LsaOpenPolicy(ref systemName, ref objectAttributes, desiredAccess, out policyHandle);
            winErrorCode = LsaNtStatusToWinError(resultPolicy);

            if (winErrorCode != NO_ERROR)
            {
                errorMessage = string.Format("OpenPolicy failed: {0} ", winErrorCode);
                throw new Win32Exception(winErrorCode, errorMessage);
            }
            else
            {
                try
                {
                    LSA_UNICODE_STRING[] userRights = new LSA_UNICODE_STRING[1];
                    userRights[0] = new LSA_UNICODE_STRING();
                    userRights[0].SetTo(privilegeName);

                    if (bRemove)
                    {
                        // Removes a privilege from an account
                        uint result = LsaRemoveAccountRights(policyHandle, pSid, false, userRights, 1);
                        winErrorCode = LsaNtStatusToWinError(result);
                        if (winErrorCode != NO_ERROR)
                        {
                            errorMessage = string.Format("LsaRemoveAccountRights failed: {0}", winErrorCode);
                            throw new Win32Exception((int)winErrorCode, errorMessage);
                        }
                    }
                    else
                    {
                        // Adds a privilege to an account
                        uint res = LsaAddAccountRights(policyHandle, pSid, userRights, 1);
                        winErrorCode = LsaNtStatusToWinError(res);
                        if (winErrorCode != 0)
                        {
                            errorMessage = string.Format("LsaAddAccountRights failed: {0}", winErrorCode);
                            throw new Win32Exception((int)winErrorCode, errorMessage);
                        }
                    }
                }
                finally
                {
                    LsaClose(policyHandle);
                }
            }
            FreeSid(pSid);

        }
    }
}
