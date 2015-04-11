using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Xml;

namespace winsw
{
    /// <summary>
    /// Specify the download activities prior to the launch.
    /// This enables self-updating services.
    /// </summary>
    public class Download
    {
        public readonly string Checksum;
        public readonly string From;
        public readonly string To;

        internal Download(XmlNode n)
        {
            From = Environment.ExpandEnvironmentVariables(n.Attributes["from"].Value);
            To = Environment.ExpandEnvironmentVariables(n.Attributes["to"].Value);
            Checksum = Environment.ExpandEnvironmentVariables(n.Attributes["checksum"].Value);
        }

        public void Perform()
        {
            WebRequest req = WebRequest.Create(From);
            WebResponse rsp = req.GetResponse();
            FileStream tmpstream = new FileStream(To + ".tmp", FileMode.Create);
            var md5Hash = CopyStream(rsp.GetResponseStream(), tmpstream);
            if (!string.IsNullOrEmpty(Checksum) && !string.Equals(Checksum, md5Hash, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception(string.Format("The downloaded file has checksum '{0}', but expected '{1}'", md5Hash, Checksum));
            }

            // only after we successfully downloaded a file, overwrite the existing one
            if (File.Exists(To))
                File.Delete(To);
            File.Move(To + ".tmp", To);
        }

        // Compute hash while copying http://stackoverflow.com/a/3621316/1644019
        public static string CopyStream(Stream i, Stream o)
        {
            HashAlgorithm hasher = MD5.Create();
            byte[] buf = new byte[8192];
            while (true)
            {
                int len = i.Read(buf, 0, buf.Length);
                if (len <= 0) break;
                hasher.TransformBlock(buf, 0, len, null, 0);
                o.Write(buf, 0, len);
            }
            
            i.Close();
            o.Close();

            hasher.TransformFinalBlock(new byte[0], 0, 0);
            return BitConverter.ToString(hasher.Hash).Replace("-", "");
        }
    }
}
