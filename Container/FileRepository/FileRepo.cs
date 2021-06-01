using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon;
using Amazon.Runtime;
using Container.Secrets;
using Amazon.S3.Transfer;
using System.Configuration;
using System.IO;
using System.Web.Mvc;

namespace Container.FileRepository
{
    public class FileRepo
    {
        public async Task<string> testListBucketsAsync()
        {
            string r = "";
            var s3Client = new AmazonS3Client(S3.awsAccessKeyId,
                S3.awsSecretAccessKey, S3.regionEndpoint);
            var listResponse = await s3Client.ListBucketsAsync();
            foreach (S3Bucket b in listResponse.Buckets)
            {
                r+=b.BucketName;
            }           
            return r;
        }

        public async Task<bool> upload(HttpPostedFileBase file, string path)
        {
            file.SaveAs(path);
            var s3Client = new AmazonS3Client(S3.awsAccessKeyId, S3.awsSecretAccessKey, S3.regionEndpoint);
            bool success = false;
            string m = "";
            var fileTransferUtility = new TransferUtility(s3Client);
            try
            {
                if (file.ContentLength > 0)
                {
                    var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = "ar-container-bucket",
                        FilePath = path,
                        PartSize = 256,//256 Bytes 
                        Key = file.FileName,
                        CannedACL = S3CannedACL.Private
                    };
                    //fileTransferUtilityRequest.Metadata.Add("param1", "Value1");
                   
                    fileTransferUtility.Upload(fileTransferUtilityRequest);
                    fileTransferUtility.Dispose();
                    File.Delete(path);
                }
                success = true;
            }

            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    m = "Check the provided AWS Credentials.";
                }
                else
                {
                    m = "Error occurred: " + amazonS3Exception.Message;
                }
            }
            return success;
        }
    }
}