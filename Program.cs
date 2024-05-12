using System;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.IO.Compression;


namespace DWUpdate
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static async Task Main()
        {
            // Get current version of Decima Workshop if it exists
            if (Directory.Exists(@"Decima\lib") && File.Exists(@"Decima\decima.bat")) // Decima Worksop is installed
            {
                // Check if Decima Workshop is installed
                string path = @"Decima\decima.bat";
                Installed installed = new Installed();
                string ver = installed.Get(path);

                // Get the latest version of Decima Workshop from GitHub
                string uri = "https://api.github.com/repos/ShadelessFox/decima/releases/latest";
                CheckVersion cv = new CheckVersion();
                string _ver = await cv.Get(uri);
                GetUrl gu = new GetUrl();
                string url = await gu.Get(uri);
                string folderName = @"decima-" + _ver; // decima-*.*.*

                // If a new release is available
                if (_ver != null && ver != null && ver != _ver)
                {
                    // Ask the user if they want to download the latest release
                    DialogResult result = MessageBox.Show(
                    $"Found new version of Decima Workshop. Installed version: {ver}. Would you like download the latest release?",
                    $"Decima Workshop {_ver} available!",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);

                    // Decima Worksop is present and OK button is clicked
                    if (result == DialogResult.OK)
                    {
                        // Create a unique name
                        string tempName = Guid.NewGuid().ToString();

                        // Set the path of the temporary folder with the unique name
                        string tmp = Path.Combine(Path.GetTempPath(), tempName);

                        // Get the full path of the Decima\lib folder
                        string lib = Path.GetFullPath(@"Decima\lib");

                        // Create the temporary folder with the unique name
                        Directory.CreateDirectory(tmp);

                        // Create the Decima folder
                        Directory.CreateDirectory(@"Decima");

                        // Set the path of the Decima folder as target
                        string unpack = (@"Decima");

                        // Download the zip file
                        Download d = new Download();
                        string zipPath = await d.Get(url, tmp, $"{tempName}.zip");

                        // Set the path of the zip file as source
                        string SourcePath = Path.Combine(tmp, folderName);

                        // The absolute path of the destination folder (@"Decima")
                        string DestinationPath = Path.GetFullPath(unpack);
                        
                        // Delete the Decima\lib folder for supressing created myltiply .jar files
                        Directory.Delete(lib, true);

                        // Unzip the downloaded zip file
                        UnZip uz = new UnZip();
                        uz.F2F(tmp, zipPath, SourcePath, DestinationPath); // распаковка

                        // Show update success message
                        MessageBox.Show(
                        $"Decima Workshop {ver} successfully updated to {_ver}.",
                        "Install complete!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.DefaultDesktopOnly);
                    }
                };
            }
            else  // Decima Workshop absent
            {
                try
                {
                    string uri = "https://api.github.com/repos/ShadelessFox/decima/releases/latest";
                    CheckVersion cv = new CheckVersion();
                    string _ver = await cv.Get(uri);
                    GetUrl gu = new GetUrl();
                    string url = await gu.Get(uri);
                    string folderName = @"decima-" + _ver; // decima-*.*.*

                    DialogResult result = MessageBox.Show(
                    "Decima Workshop not found! Would you like download the latest version?",
                    "Decima Workshop not found!",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);

                    // Decima Worksop is present and OK button is clicked
                    if (result == DialogResult.OK)
                    {
                        string tempName = Guid.NewGuid().ToString();
                        string tmp = Path.Combine(Path.GetTempPath(), tempName);
                        Directory.CreateDirectory(tmp); 
                        Directory.CreateDirectory(@"Decima");
                        string unpack = (@"Decima");
                        Download d = new Download();
                        string zipPath = await d.Get(url, tmp, $"{tempName}.zip");
                        string SourcePath = Path.Combine(tmp, folderName);
                        string DestinationPath = Path.GetFullPath(unpack);
                        UnZip uz = new UnZip();
                        uz.F2F(tmp, zipPath, SourcePath, DestinationPath);

                        MessageBox.Show(
                        $"Decima Workshop {_ver} successfully installed.",
                        "Install complete!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.DefaultDesktopOnly);
                    }
                    else // Decima Worksop is absent and Cancel button is clicked
                    {
                        // Force exit the program
                        Environment.Exit(0);
                    }
                }
                catch (Exception e) // catch exception
                {
                    // Show an error message if an exception occurs
                    MessageBox.Show(
                    "Reason[0]: " + e.Message,
                    "Oops... Something wrong...",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);
                    
                    // Force exit the program
                    Environment.Exit(0);
                };
            };
            //Exit the program
            Application.Exit();
        }
    }
   
    public class CheckVersion
    {
        /// <summary>
        /// Entry point for retrieving the version of the latest release.
        /// </summary>
        /// <param name="uri">The URI of the GitHub repository.</param>
        /// <returns>The version of the latest release.</returns>
        public async Task<string> Get(string uri)
        {
            try
            {
                // Create a new HttpClient instance
                var httpClient = new HttpClient();

                // Clear the default accept header and add the GitHub API accept header
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json")); // ПРИНЯТЬ заголовок

                // Set the request URI
                string request = uri;

                // Add a User-Agent header to the request to avoid a 403 error
                httpClient.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter"); // БЕЗ ЭТОГО ОШИБКА 403

                // Send a GET request to the URI and ensure that the request was successful
                HttpResponseMessage trap = (await httpClient.GetAsync(request, HttpCompletionOption.ResponseHeadersRead)).EnsureSuccessStatusCode();

                // Send a GET request to the URI and ensure that the request was successful, and read the response content
                var response = httpClient.GetStringAsync(request);

                // Get the response content as a string
                var jArray = await response;

                // Get the tag name from the response
                string tag = JObject.Parse(jArray).SelectToken("tag_name").ToString();

                // Split the tag name into its components
                string[] _tokens = tag.Split('.');

                // Convert the component tokens to integers
                List<int> _numbers = new List<int>();
                foreach (string _token in _tokens)
                {
                    if (int.TryParse(string.Concat(_token.Where(c => char.IsDigit(c))), out int _number))
                        _numbers.Add(_number);
                };
                // Extract the major, minor, and build numbers from the list of integers
                int _id0 = _numbers[0]; // Major
                int _id1 = _numbers[1]; // Minor
                int _id2 = _numbers[2]; // Build

                // Construct the version string
                string _ver = ($"{_id0}.{_id1}.{_id2}");

                // Return the version string
                return (_ver);
            }
            catch (Exception e)
            {
                // If an exception occurs, show an error message and return null
                MessageBox.Show(
                $"Exception in CheckVersion function! Reason[0]: {e.Message}",
                "Oops... Something wrong...",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);
                return null;
            }
        }
    }

    /// <summary>
    /// This class is responsible for retrieving the URL of the latest release from a GitHub repository.
    /// </summary>
    public class GetUrl
    {
        /// <summary>
        /// Entry point for retrieving the URL of the latest release.
        /// </summary>
        /// <param name="uri">The URI of the GitHub repository.</param>
        /// <returns>The URL of the latest release, or null if an exception occurs.</returns>
        public async Task<string> Get(string uri)
        {
            try
            {
               // Create a new HttpClient instance
                var httpClient = new HttpClient();

                // Clear the default accept header and add the GitHub API accept header
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

                // Set the request URI
                string request = uri;

                // Add a User-Agent header to the request to avoid a 403 error
                httpClient.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

                // Send a GET request to the URI and ensure that the request was successful
                HttpResponseMessage trap = (await httpClient.GetAsync(request, HttpCompletionOption.ResponseHeadersRead)).EnsureSuccessStatusCode();

                // Get the response content as a string
                var response = await httpClient.GetStringAsync(request);

                // Parse the response as a JObject
                var jArray = JObject.Parse(response);

                // Get the URL of the first asset in the release
                string url = jArray.SelectToken("assets[0].browser_download_url").ToString();

                // Return the URL
                return url;
                }
                catch (Exception e)
                {
                // If an exception occurs, show an error message and return null
                MessageBox.Show(
                $"Exception in GetUrl function! Reason[0]: {e.Message}",
                "Oops... Something wrong...",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);
                return null;
            }
        }
    }

    /// <summary>
    /// Class responsible for downloading files from a given URL.
    /// </summary>
    public class Download
    {
        /// <summary>
        /// Entry point for the download process.
        /// </summary>
        /// <param name="url">The URL to download the file from.</param>
        /// <param name="path">The path where the downloaded file will be saved.</param>
        /// <param name="zip">The name of the downloaded file.</param>
        /// <returns>The path where the downloaded file is saved, or null if an exception occurred.</returns>
        public async Task<string> Get(string url, string path, string zip)
        {
            try
            {
                // Create a new HttpClient instance
                var httpClient = new HttpClient();

                // Clear the default accept headers and add the accept header for application/zip
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/zip"));

                // Create the request URI
                string request = url;

                // Add the user agent header to avoid 403 errors
                httpClient.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

                // Send a request to the server and ensure the response is successful
                HttpResponseMessage trap = (await httpClient.GetAsync(request, HttpCompletionOption.ResponseHeadersRead)).EnsureSuccessStatusCode();

                // Send a request to the server and ensure the response is successful, and read the response content
                var response = (await httpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead)).EnsureSuccessStatusCode();

                // Read the response content as a stream
                var stream = await response.Content.ReadAsStreamAsync();

                // Combine the path and zip name to get the full path where the downloaded file will be saved
                string zipPath = Path.Combine(path, zip);

                // Create a new file at the zip path and copy the stream content to the file
                using (var saveFile = File.Create(zipPath))
                {
                    await stream.CopyToAsync(saveFile);
                    saveFile.Flush();
                }
                    return zipPath; // Return the full path of the downloaded file
            }
            catch (Exception e)
            {
                MessageBox.Show(
                $"Exception in Download function! Reason[0]: {e.Message}",
                "Oops... Something wrong...",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);
                return null;
            }
        }
    }

    /// <summary>
    /// The UnZip class is responsible for unzipping files.
    /// </summary>
    public class UnZip
    {
        /// <summary>
        /// Entry point for the unzipping process.
        /// </summary>
        /// <param name="temp">The temporary directory where the files will be extracted.</param>
        /// <param name="zipPath">The path to the zip file to be extracted.</param>
        /// <param name="sourcePath">The source directory where the files are located before extraction.</param>
        /// <param name="destinationPath">The destination directory where the files will be extracted to.</param>
        public void F2F(string temp, string zipPath, string sourcePath, string destinationPath)
        {
            try
            {
                // Open the zip file and extract its contents to the temporary directory
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    archive.ExtractToDirectory(temp);
                }

                // This is a workaround for the poor System.IO.Compression library
                // Create identical directory trees
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

                // Copy all files and overwrite if they already exist
                foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
            }
            catch (Exception e)
            {
                // Show an error message if an exception occurs
                MessageBox.Show(
                $"Exception in UnZip function! Reason[0]: {e.Message}",
                "Oops... Something wrong...",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);

                // Delete the temporary directory if exception occurs
                Directory.Delete(temp, true);
            }
            // Delete the temporary directory after extraction
            Directory.Delete(temp, true);
        }
    }

    /// <summary>
    /// This class is responsible for getting the installed version of a software.
    /// </summary>
    public class Installed
    {
        /// <summary>
        /// Entry point for checking the installed version.
        /// </summary>
        /// <param name="file">The path to the file to be checked.</param>
        /// <returns>The version of the software installed, if found. Otherwise, null.</returns>
        public string Get(string file)
        {
            try
            {
                // Define the name of the environment variable and the path to be checked in the classpath
                string classpathVariable = "CLASSPATH";
                string libPath = "%APP_HOME%\\lib\\";

                // Read all lines from the file
                string[] lines = File.ReadAllLines(file);
                string classpathValue = "";

                // Iterate over each line to find the line that starts with "set " followed by the classpathVariable
                foreach (string line in lines)
                {
                    if (line.StartsWith("set " + classpathVariable))
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            classpathValue = parts[1].Trim();
                            break; // Exit the loop as soon as the classpathValue is found
                        }
                    }
                }

                // If the classpathValue exists and contains the libPath, extract the version from it
                if (!string.IsNullOrEmpty(classpathValue) && classpathValue.Contains(libPath))
                {
                    // Find the index where the libPath starts in the classpathValue
                    int startIndex = classpathValue.IndexOf(libPath) + libPath.Length;

                    // Get the remaining part of the classpathValue after the libPath
                   string remainingPath = classpathValue.Substring(startIndex);

                    // Split the remainingPath into parts by ';'
                    string[] parts = remainingPath.Split(';');
                    string fileName = parts[0];
                    string mainFile = Path.GetFileNameWithoutExtension(fileName);

                    // Split the mainFile into tokens by '.'
                    string[] tokens = mainFile.Split('.');
                    List<int> numbers = new List<int>();
                    foreach (string token in tokens)
                    {
                        // Try to parse each token as int and add it to numbers if parsing is successful
                        if (int.TryParse(string.Concat(token.Where(c => char.IsDigit(c))), out int number))
                        numbers.Add(number);
                    };

                    int id0 = numbers[0]; // Major
                    int id1 = numbers[1]; // Minor
                    int id2 = numbers[2]; // Build
                    string ver = ($"{id0}.{id1}.{id2}");
                    return (ver);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(
                $"Exception in Installed function! Reason: {e.Message}",
                "Oops... Something wrong...",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);
                return null;
            }
        }
    }
};