using System.IO;
#if UNITY_EDITOR || !UNITY_IPHONE
using Ionic.Zip;
#endif

namespace HihiFramework.Core {
    public class ZipUtils {
#if !UNITY_EDITOR && UNITY_IPHONE
	[DllImport("__Internal")]
	private static extern void unzip (string zipFilePath, string location);

	[DllImport("__Internal")]
	private static extern void zip (string zipFilePath);

	[DllImport("__Internal")]
	private static extern void addZipFile (string addFile);

#endif

        /// <summary>
        /// It <b>will override</b> original file in destFolder if existed
        /// </summary>
        public static void Unzip (string zipFilePath, string destFolder) {
#if !UNITY_EDITOR && UNITY_IPHONE
            unzip (zipFilePath, destFolder);
#else
            Directory.CreateDirectory (destFolder);

            using (var zip = ZipFile.Read (zipFilePath)) {
                zip.ExtractAll (destFolder, ExtractExistingFileAction.OverwriteSilently);
            }
#endif
        }

        public static void Zip (string zipFileName, params string[] files) {
#if !UNITY_EDITOR && UNITY_IPHONE
		    foreach (var file in files) {
			    addZipFile (file);
		    }
		    zip (zipFileName);
#elif !UNITY_EDITOR && UNITY_ANDROID
            using (var zipper = new AndroidJavaClass ("com.tsw.zipper")) {
				zipper.CallStatic ("zip", zipFileName, files);
			}
#else
            var path = Path.GetDirectoryName (zipFileName);
            Directory.CreateDirectory (path);

            using (var zip = new ZipFile ()) {
                foreach (var file in files) {
                    zip.AddFile (file, "");
                }
                zip.Save (zipFileName);
            }
#endif
        }
    }
}