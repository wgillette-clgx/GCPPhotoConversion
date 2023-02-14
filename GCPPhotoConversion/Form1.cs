using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GCPPhotoConversion
{
    public partial class Form1 : Form
    {
        const string DATABASE_CONNECTION = "Server=aggdb01-mls-uat.infosolco.com;Database=Global_Cincinnati;Trusted_Connection=True;";
        const string WORK_FOLDER = @"D:\MLS_Photorename_Workfolder";
        const string JsonFilePath = @"C:\Users\wgillette\Photo.json";
        DataTable ListToDownload = new DataTable();
        #region getdata
        string MyQuery = 
       $@"Declare @SQL nvarchar (max),
        @Dbname Nvarchar(50),
		@MLsId Nvarchar(10),
		@IsActive Nvarchar(1),
		@isActive_Product Nvarchar(1)
        if(object_id('global_cincinnati.dbo.HoldConvertPhotoCount')is not null)
	        drop table global_cincinnati.dbo.HoldConvertPhotoCount

        create table global_cincinnati.dbo.HoldConvertPhotoCount
        (
        RecID int identity(1,1),
        Dbname Varchar(50),
        MlsId varchar(10),
        IsActive Nvarchar(1),
        isActive_Product Nvarchar(1),
		oldListingphoto varchar(200),
		newListingphoto varchar(200),
		oldphoto varchar(200),
		newphoto varchar(200),
		oldimagelink varchar(200),
		newimagelink varchar(200),
		FileStatus int default(0), 
        Fa_listid Varchar(100),
        Runtime DateTime2(2)
        )
        declare PhotoCsr cursor fast_forward for 
        Select  DBname, MLSID, IsActive, isActive_Product from global_cincinnati..tbmls where isphotosactive =1 and db_id(dbname) is not null  and dbname = 'MLS_rebny' order by dbname
          open PhotoCsr
          Fetch Next From PhotoCsr into @DbName,@mlsId,@IsActive,@isActive_Product
          While @@FETCH_STATUS = 0
        begin
         set @SQL = concat(' use ', @DbName,'
         if(object_id(''tblistingphoto'') is not null)
         Insert into global_cincinnati.dbo.HoldConvertPhotoCount(
		 Dbname, 
		 MlsId, 
		 IsActive, 
		 isActive_Product,
		 oldListingphoto,
		 newListingphoto,
		 oldphoto,
		 newphoto,
		 oldimagelink,
		 newimagelink,
		 Fa_listid,
		 Runtime)
         Select top 10 
		 ''',@DbName,''',
		 ',@mlsId,',
		 ',@IsActive,',
		 ',@isActive_Product,',
		 PhotoFullPathName,
		 Concat(reverse(substring(reverse(photofullpathname),(charindex(''.'',reverse(photofullpathname),1)),len(photofullpathname))),''jpg''),
		 reverse(substring(reverse(photofullpathname),1,Charindex(''\'',reverse(photofullpathname),1)-1)),
		 Concat(reverse(SUBSTRING(substring(reverse(photofullpathname),(charindex(''.'',reverse(photofullpathname),1)),len(photofullpathname)),1,Charindex(''\'',substring(reverse(photofullpathname),(charindex(''.'',reverse(photofullpathname),1)),len(photofullpathname)),1)-1)),''jpg''), 
		 replace(replace(photofullpathname,''\\Live-PhotoShare-MLS.infosolco.com'',''''),''\'',''/''),
		 replace(replace(Concat(reverse(substring(reverse(photofullpathname),(charindex(''.'',reverse(photofullpathname),1)),len(photofullpathname))),''jpg''),''\\Live-PhotoShare-MLS.infosolco.com'',''''),''\'',''/''),
		 Fa_listid,
		 getdate()
        from tblistingphoto
        where photofullpathname not like ''%.jpg''')
         exec(@SQL)
        Fetch Next From PhotoCsr into @DbName, @mlsId, @IsActive, @isActive_Product
        end
        close PhotoCsr
        deallocate PhotoCsr";
        #endregion
        
        public Form1()
        {
            InitializeComponent();
        }

        private void btnGetData_Click(object sender, EventArgs e)
        {
            using(SqlConnection con = new SqlConnection(DATABASE_CONNECTION))
            {
                using (SqlCommand com = new SqlCommand())
                {
                    con.Open();
                    com.Connection = con;
                    com.CommandTimeout = 120;
                    com.CommandText = MyQuery;
                    com.CommandType = CommandType.Text;
                    int retVal = Convert.ToInt32(com.ExecuteScalar());
                }
            }
        }
        private DataTable GetListOfFiles()
        {
            DataTable MyData = new DataTable();
            string QueryData = @"Select *
                                 From global_cincinnati.dbo.HoldConvertPhotoCount
                                 where FileStatus = 0";

            using (SqlConnection con = new SqlConnection(DATABASE_CONNECTION))
            {
                using (SqlCommand com = new SqlCommand())
                {
                    
                    con.Open();
                    com.Connection = con;
                    com.CommandTimeout = 120;
                    com.CommandText = QueryData;
                    com.CommandType = CommandType.Text;

                    SqlDataAdapter Da = new SqlDataAdapter(com);
                    Da.Fill(MyData);
                }
            }
            return MyData;
        }

        private void UpdateTable(string Table,string updateField,string oldValue, string NewValue, string Falistid,string DbName)
        {
            string DbConn = $"Server=aggdb01-mls-uat.infosolco.com;Database={DbName};Trusted_Connection=True;";
            string UpdateQuicksearch = $"Update {Table} set {updateField} = '{NewValue}' where fa_listid ='{Falistid}' and {updateField}='{oldValue}'";
            using(SqlConnection conn = new SqlConnection(DbConn))
            {
                using (SqlCommand com = new SqlCommand())
                {
                    com.Connection = conn;
                    com.CommandText = UpdateQuicksearch;
                    com.CommandType = CommandType.Text;
                    conn.Open();
                    com.ExecuteNonQuery();
                }
            }
        }
        private bool UpdateMainTable(string RecId,int Status)
        {
            string UpdateQuicksearch = $"Update global_cincinnati.dbo.HoldConvertPhotoCount set FileStatus = {Status} where RecID ='{RecId}'; Select @@Rowcount";
            using (SqlConnection conn = new SqlConnection(DATABASE_CONNECTION))
            {
                using (SqlCommand com = new SqlCommand())
                {
                    com.Connection = conn;
                    com.CommandText = UpdateQuicksearch;
                    com.CommandType = CommandType.Text;
                    conn.Open();
                    int updatedRows = (int)com.ExecuteScalar();
                    return updatedRows > 0;
                }
            }
        }
        private void DownloadDataFromGCP(string PhotoLink)
        {
            if (!Directory.Exists(WORK_FOLDER))
                Directory.CreateDirectory(WORK_FOLDER);

            string DownloadCmd = $@"gsutil cp {PhotoLink.Replace(@"\\Live-PhotoShare-MLS.infosolco.com\", @"gs://madl-mlsphoto-prd-gcs01/").Replace("\\", "/")} {WORK_FOLDER}";
            RunCommand(DownloadCmd);
            string newpath = Path.Combine(WORK_FOLDER,Path.GetFileName(PhotoLink));
            string renamepath = Path.Combine(WORK_FOLDER,Path.GetFileNameWithoutExtension(newpath) + ".jpg");
            Bitmap bm = new Bitmap(newpath);
            if(Path.GetExtension(PhotoLink).Equals(".png", StringComparison.CurrentCultureIgnoreCase))
                bm.SaveJpeg(renamepath, 50L);
            else
                bm.SaveJpeg(renamepath, 100L);
        }
        static void RunCommand(string cmd)
        {
            ProcessStartInfo RunCmdInfo = new ProcessStartInfo();
            Process RunCmd = new Process();
            
            RunCmdInfo.FileName = Path.Combine(Environment.SystemDirectory, @"cmd.exe");
            RunCmdInfo.Arguments = $@"/C gcloud auth activate-service-account --key-file {JsonFilePath} &&" +
                                   $" {cmd} ";
            RunCmdInfo.CreateNoWindow = true;
            RunCmdInfo.UseShellExecute = false;
            RunCmdInfo.RedirectStandardOutput = true;
            //RunCmdInfo.RedirectStandardInput = true;
            RunCmdInfo.RedirectStandardError = true;
            RunCmd.StartInfo = RunCmdInfo;
            try
            {
                RunCmd.Start();
                string resultOutput = RunCmd.StandardOutput.ReadToEnd();
                if (RunCmd.ExitCode != 0)
                    throw new Exception(RunCmd.StandardError.ReadToEnd());

                RunCmd.WaitForExit();

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void btnGetList_Click(object sender, EventArgs e)
        {
            ListToDownload =  GetListOfFiles();
        }

        private void btnDownloadPhotos_Click(object sender, EventArgs e)
        {
            foreach (DataRow row in ListToDownload.Rows)
            {
                //download file from GCP and change extension
                DownloadDataFromGCP(row["oldListingphoto"].ToString());
                //update status
                if(UpdateMainTable(row["RecId"].ToString(),1))
                { 
                    //upload file to GCP
                    UploadDataToGCP(row["newListingphoto"].ToString());
                    //Change Status on main table
                    if (UpdateMainTable(row["RecId"].ToString(), 2))
                    {
                        //update quicksearch
                        UpdateTable("Tbfa_quickSearch",
                                    "imageLink1",
                                    row["oldimagelink"].ToString(),
                                    row["newimagelink"].ToString(),
                                    row["Fa_Listid"].ToString(),
                                    row["DbName"].ToString());
                        UpdateMainTable(row["RecId"].ToString(), 2);

                        //update tblistinghoto
                        UpdateTable("TbListingPhoto",
                                    "PhotoFullPathName",
                                    row["oldListingphoto"].ToString(),
                                    row["newListingphoto"].ToString(),
                                    row["Fa_Listid"].ToString(),
                                    row["DbName"].ToString());
                        UpdateMainTable(row["RecId"].ToString(), 2);

                        //update tbphoto
                        UpdateTable("TbPhotos",
                                    "PrimaryPhotoFileName",
                                    row["oldphoto"].ToString(),
                                    row["newphoto"].ToString(),
                                    row["Fa_Listid"].ToString(),
                                    row["DbName"].ToString());
                        UpdateMainTable(row["RecId"].ToString(), 2);
                        //delete file from gcp
                        RemoveFileFromGCP(row["oldListingphoto"].ToString());
                        UpdateMainTable(row["RecId"].ToString(), 2);
                    }
                }
            }
        }

        private void UploadDataToGCP(string PhotoLink)
        {
            //PhotoLink = Path.ChangeExtension(PhotoLink, "jpg");
            string localFile = Path.Combine(WORK_FOLDER, Path.GetFileName(PhotoLink));
            string GcpLocation = $"{PhotoLink.Replace(@"\\Live-PhotoShare-MLS.infosolco.com\", @"gs://madl-mlsphoto-prd-gcs01/").Replace("\\", "/") }";
            
            string UploadCmd = $@"gsutil -m mv {localFile} {GcpLocation}";
            RunCommand(UploadCmd);
        }

        private void RemoveFileFromGCP(string PhotoLink)
        {
            string GcpLocation = $"{PhotoLink.Replace(@"\\Live-PhotoShare-MLS.infosolco.com\", @"gs://madl-mlsphoto-prd-gcs01/").Replace("\\", "/") }";

            string UploadCmd = $@"gsutil rm {GcpLocation}";
            RunCommand(UploadCmd);
        }

    }
    public static class ImageExtensions
    {
        public static void SaveJpeg(this Image img, string filePath, long quality)
        {
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            img.Save(filePath, GetEncoder(ImageFormat.Jpeg), encoderParameters);
        }

        public static void SaveJpeg(this Image img, Stream stream, long quality)
        {
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            img.Save(stream, GetEncoder(ImageFormat.Jpeg), encoderParameters);
        }

        static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            return codecs.Single(codec => codec.FormatID == format.Guid);
        }
    }
}
