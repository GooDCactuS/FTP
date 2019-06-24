using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;

namespace FTP
{
    public class FtpClient
    {
        private string password;
        private string userName;
        private string uri;


        public FtpClient(string uri, string userName, string password)
        {
            this.uri = uri;
            this.userName = userName;
            this.password = password;
        }

        public List<FileDir> ListDirectoryDetails(string directory = "")
        {
            var list = new List<string>();
            // Запрос списка файлов
            FtpWebRequest request;
            if (directory == "")
                request = createRequest(uri, WebRequestMethods.Ftp.ListDirectoryDetails);
            else
                request = createRequest(directory, WebRequestMethods.Ftp.ListDirectoryDetails);

            // Ответ сервера
            var response = (FtpWebResponse)request.GetResponse();
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream, true);

            // Список файлов из ответа сервера
            while (!reader.EndOfStream)
                list.Add(reader.ReadLine());
            MainWindow.UpdateStatus = response.StatusDescription;
            return TransformFileInfo(list.ToArray(), directory);
        }

        private FtpWebRequest createRequest(string uri, string method)
        {
            var r = (FtpWebRequest)WebRequest.Create(uri);

            r.Credentials = new NetworkCredential(userName, password);
            r.Method = method;
            
            return r;
        }

        private List<FileDir> TransformFileInfo(string[] fileList, string directory="")
        {
            // Регулярное выражение, которое ищет информацию о папках и файлах 
            // в строке ответа от сервера
            Regex regex = new Regex(@"^([d-])([rwxt-]{3}){3}\s+\d{1,}\s+.*?(\d{1,})\s+(\w+\s+\d{1,2}\s+(?:\d{4})?)(\d{1,2}:\d{2})?\s+(.+?)\s?$",
                RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            // Получаем список корневых файлов и папок
            // Используется LINQ to Objects и регулярные выражения
            List<FileDir> list = fileList.Select(s =>
                                                     {
                                                         Match match = regex.Match(s);
                                                         if (match.Length > 5)
                                                         {
                                                             // Устанавливаем тип, чтобы отличить файл от папки 
                                                             string type = match.Groups[1].Value == "d" ? "directory" : "file";

                                                             // Размер задаем только для файлов, т.к. для папок возвращается
                                                             // размер ярлыка 4кб, а не самой папки
                                                             string size = "";
                                                             if (type == "file")
                                                                 size = (Int32.Parse(match.Groups[3].Value.Trim()) / 1024).ToString() + " kB";
                                                             if(directory=="")
                                                                return new FileDir(size, type, match.Groups[6].Value, match.Groups[4].Value, MainWindow.window.txt_address.Text);
                                                             else
                                                                return new FileDir(size, type, match.Groups[6].Value, match.Groups[4].Value, directory);
                                                         }
                                                         else return new FileDir();
                                                     }).ToList(); 

            return list;
        }

        public void DeleteFile(string fileUri)
        {
            var request = createRequest(fileUri, WebRequestMethods.Ftp.DeleteFile);
            var response = (FtpWebResponse)request.GetResponse();
            MainWindow.UpdateStatus = response.StatusDescription;
        }



        public void DeleteSubdirectoryFiles(string directoryUri, string chosenType)
        {
            List<FileDir> list = MainWindow.window.client.ListDirectoryDetails(directoryUri);
            foreach (var item in list)
            {
                if (item.Type == "file")
                {
                    string fileName = item.Name;
                    int index = fileName.LastIndexOf('.');
                    string fileType = fileName.Substring(index + 1);
                    if (fileType == chosenType)
                    {
                        try
                        {
                            MainWindow.window.client.DeleteFile(item.address + item.Name);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString() + ": \n" + ex.Message);
                        }
                        
                    }
                }
                else if(item.Type == "directory")
                {
                    DeleteSubdirectoryFiles(directoryUri + item.Name + "/", chosenType);
                }
            }
        }
    }
}