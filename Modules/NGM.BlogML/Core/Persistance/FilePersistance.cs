using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace NGM.BlogML.Core.Persistance {
    public class FilePersistance {
        public string UploadMediaFile(string target, byte[] data) {
            var targetLocation = HttpContext.Current.Server.MapPath(target);

            if (File.Exists(targetLocation))
                return null; // TODO: Delete file??

            if (!Directory.Exists(Path.GetDirectoryName(targetLocation)))
                Directory.CreateDirectory(Path.GetDirectoryName(targetLocation));

            using (var fileStream = new FileStream(targetLocation, FileMode.CreateNew)) {
                using (var binaryWriter = new BinaryWriter(fileStream)) {
                    binaryWriter.Write(data);
                }
            }

            return target;
        }
    }
}