using System;
using System.IO;
using System.IO.Packaging;
using Microsoft.Deployment.Common.Model;

namespace Microsoft.Deployment.Common.Helpers
{
    public class PBIXUtils : IDisposable
    {
        private string _originalFile;
        private string _modifiedFile;

        private ZipPackage _pbixPackage;

        private ZipPackagePart _mashup;
        private ZipPackagePart _model;

        public PBIXUtils() { }

        public PBIXUtils(string pbixSourceFileName, string pbixTargetFileName)
        {
            _originalFile = pbixSourceFileName;
            _modifiedFile = pbixTargetFileName;

            File.Copy(_originalFile, _modifiedFile, true);

            _pbixPackage = (ZipPackage)ZipPackage.Open(_modifiedFile, FileMode.Open);
            var enumerator = _pbixPackage.GetParts().GetEnumerator();

            if (enumerator == null) return;

            while (enumerator.MoveNext() && (_mashup == null || _model == null))
            {
                var currentPart = enumerator.Current as ZipPackagePart;
                if (currentPart == null) continue;

                if (currentPart.Uri.OriginalString.Contains("/DataMashup"))
                    _mashup = currentPart;
                else if (currentPart.Uri.OriginalString.Contains("/DataModel"))
                    _model = currentPart;
            }

        }


        public void ReplaceKnownVariableinMashup(string variable, string newValue)
        {
            byte[] mashupBytes;
            using (var memoryStream = new MemoryStream())
            {
                _mashup.GetStream().CopyTo(memoryStream);
                mashupBytes = memoryStream.ToArray();
            }

            QDEFF qdeff = new QDEFF(mashupBytes);
            qdeff.ReplaceKnownVariable(variable, newValue);

            byte[] newContent = qdeff.RecreateContent();

            _mashup.GetStream().Write(newContent, 0, newContent.Length);
            _mashup.GetStream().SetLength(newContent.Length);
            _mashup.GetStream().Flush();
            _pbixPackage.Flush();
        }


        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _pbixPackage?.Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PBIXUtils() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}