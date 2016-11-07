using System;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Deployment.Common.Model
{

    public class QDEFF
    {
        private uint _packagePartsLength;
        private byte[] _packageParts;

        private uint _permissionsLength;
        private byte[] _permissions;

        private uint _metaDataLength;
        private byte[] _metadata;

        private uint _permissionsBindingsLength;
        private byte[] _permissionsBindings;

        private uint _length;

        private void Validate( byte[] buffer )
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "QDEFF entry: cannot be created from a null argument");
            if (buffer.Length < 20)
                throw new ArgumentOutOfRangeException(nameof(buffer), "QDEFF entry: buffer too small");
        }


        public QDEFF( byte[] contentBytes )
        {
            ParseContent(contentBytes);
        }


        private void ParseContent( byte[] contentBytes )
        {
            _length = (uint)contentBytes.Length;

            Validate(contentBytes);

            if (_packagePartsLength > _length)
                throw new InvalidDataException(
                    "Package parts: the payload is corrupt. Package parts exceeds total length");

            // The assumption is that these payloads are always little endian
            uint position = 0;
            // Advance 4 bytes, because the first uint seem to be always 0, not as documented on https://msdn.microsoft.com/en-us/library/mt577218%28v=office.12%29.aspx
            position += 4;
            _packagePartsLength = BitConverter.ToUInt32(contentBytes, (int)position);

            // Read package parts
            _packageParts = new byte[_packagePartsLength];
            position += sizeof(uint);
            Buffer.BlockCopy(contentBytes, (int)position, _packageParts, 0, (int)_packagePartsLength);
            position += _packagePartsLength;

            // Read permissions
            if (position < _length + 4)
            {
                _permissionsLength = BitConverter.ToUInt32(contentBytes, (int)position);
                position += sizeof(uint);
                _permissions = new byte[_permissionsLength];
                Buffer.BlockCopy(contentBytes, (int)position, _permissions, 0, (int)_permissionsLength);
                position += _permissionsLength;
            }

            // Read metadata
            if (position < _length + 4)
            {
                _metaDataLength = BitConverter.ToUInt32(contentBytes, (int)position);
                position += sizeof(uint);
                _metadata = new byte[_metaDataLength];
                Buffer.BlockCopy(contentBytes, (int)position, _metadata, 0, (int)_metaDataLength);
                position += _metaDataLength;
            }

            // Read permission bindings
            if (position < _length + 4)
            {
                _permissionsBindingsLength = BitConverter.ToUInt32(contentBytes, (int)position);
                position += sizeof(uint);
                _permissionsBindings = new byte[_permissionsBindingsLength];
                Buffer.BlockCopy(contentBytes, (int)position, _permissionsBindings, 0, (int)_permissionsBindingsLength);
                position += _permissionsBindingsLength;
            }
        }

        private ZipPackage GetPackageParts( string zipPackageName )
        {
            File.WriteAllBytes(zipPackageName, _packageParts);
            ZipPackage innerPackage = (ZipPackage) ZipPackage.Open(zipPackageName, FileMode.Open);

            return innerPackage;
        }

        public byte[] RecreateContent()
        {
            byte[] fullContent = new byte[sizeof (uint) +
                                          sizeof (uint) + _packagePartsLength +
                                          sizeof (uint) + _permissionsLength +
                                          sizeof (uint) + _metaDataLength +
                                          sizeof (uint) + _permissionsBindingsLength];

            uint position = 0;
            // Zero header
            byte[] numberBytes = BitConverter.GetBytes(0);
            Buffer.BlockCopy(numberBytes, 0, fullContent, (int)position, sizeof(uint));
            position += sizeof(uint);

            // Package parts
            if (_packagePartsLength == 0 || _packageParts == null)
                throw new FormatException("QDEFF entry: Package parts missing");
            numberBytes = BitConverter.GetBytes(_packagePartsLength);
            Buffer.BlockCopy(numberBytes, 0, fullContent, (int)position, sizeof(uint));
            position += sizeof(uint);
            Buffer.BlockCopy(_packageParts, 0, fullContent, (int)position, (int)_packagePartsLength);
            position += _packagePartsLength;

            // Permissions
            if (_permissionsLength == 0 || _permissions == null)
                throw new FormatException("QDEFF entry: Permissions missing");
            numberBytes = BitConverter.GetBytes(_permissionsLength);
            Buffer.BlockCopy(numberBytes, 0, fullContent, (int)position, sizeof(uint));
            position += sizeof(uint);
            Buffer.BlockCopy(_permissions, 0, fullContent, (int)position, (int)_permissionsLength);
            position += _permissionsLength;


            // Metadata
            if (_metaDataLength == 0 || _metadata == null)
                throw new FormatException("QDEFF entry: Metadata missing");
            numberBytes = BitConverter.GetBytes(_metaDataLength);
            Buffer.BlockCopy(numberBytes, 0, fullContent, (int)position, sizeof(uint));
            position += sizeof(uint);
            Buffer.BlockCopy(_metadata, 0, fullContent, (int)position, (int)_metaDataLength);
            position += _metaDataLength;


            // Permission bindings
            if (_permissionsBindingsLength == 0 || _permissionsBindings == null)
                throw new FormatException("QDEFF entry: Permission bindings missing");
            numberBytes = BitConverter.GetBytes(_permissionsBindingsLength);
            Buffer.BlockCopy(numberBytes, 0, fullContent, (int)position, sizeof(uint));
            position += sizeof(uint);
            Buffer.BlockCopy(_permissionsBindings, 0, fullContent, (int)position, (int)_permissionsBindingsLength);
            position += _permissionsBindingsLength;

            return fullContent;
        }



        public void ReplaceKnownVariable( string folder, string variable, string newValue )
        {
            string section1Path = Path.Combine(folder, "Formulas", "Section1.m");
            string mCode = File.ReadAllText(section1Path);

            // string pattern = "^shared\\s*" + variable + "\\s*=\\s*\"\\S*\"";
            string pattern = "^shared\\s*" + variable + "\\s*=\\s*\"\\S*\"";
            Regex r = new Regex(pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            string updatedMCode = r.Replace(mCode, string.Format(CultureInfo.InvariantCulture, "shared {0} = \"{1}\"", variable, newValue));

            File.WriteAllText(section1Path, updatedMCode, Encoding.UTF8);
        }

        public void ReplaceKnownVariable( string variable, string newValue )
        {
            string packagePartsTempLocation = Path.GetTempFileName();
            ZipPackage partsPackage = GetPackageParts(packagePartsTempLocation);

            var parts = partsPackage.GetParts();
            var enumerator = parts.GetEnumerator();

            ZipPackagePart mCodePart = null;
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Uri.OriginalString.Contains("/Formulas/Section1.m"))
                {
                    mCodePart = (ZipPackagePart)enumerator.Current;
                    break;
                }

            }

            string theMCode;
            using (StreamReader mCodeReader = new StreamReader(mCodePart.GetStream()))
            {
                theMCode = mCodeReader.ReadToEnd();
            }

            string pattern = "^shared\\s*" + variable + "\\s*=\\s*\"\\S*\"";
            Regex r = new Regex(pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            string updatedMCode = r.Replace(theMCode, string.Format(CultureInfo.InvariantCulture, "shared {0} = \"{1}\"", variable, newValue));

            byte[] updateMCodeBytes = Encoding.UTF8.GetBytes(updatedMCode);
            mCodePart.GetStream().Write(updateMCodeBytes, 0, updateMCodeBytes.Length);
            mCodePart.GetStream().SetLength(updateMCodeBytes.Length);

            mCodePart.GetStream().Flush();
            partsPackage.Close();

            FileInfo fi = new FileInfo(packagePartsTempLocation);
            _packagePartsLength = Convert.ToUInt32(fi.Length);
            _packageParts = File.ReadAllBytes(packagePartsTempLocation);

            File.Delete(packagePartsTempLocation);
        }
    }
}
