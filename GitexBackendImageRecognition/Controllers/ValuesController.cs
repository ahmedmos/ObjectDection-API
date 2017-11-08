using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace GitexBackendImageRecognition.Controllers
{
    [RoutePrefix("api/Values")]
    public class ValuesController : ApiController
    {
        [HttpPost]
        [Route("PostImage")]
        public  HttpResponseMessage PostImage(ImageData _imagedata)
        {
            try
            {
                string imagedata = _imagedata.Data;
                byte[] array = Convert.FromBase64String(imagedata);

                //test image folder path
                string TestImagesPath = "C:\\cntk\\Examples\\Image\\DataSets\\tools\\testImages\\";

                //store image
                Stream _stream = new MemoryStream(array);

                Bitmap _bitmap = new Bitmap(_stream);
                _bitmap.Save(TestImagesPath + _imagedata.Name + ".jpg", ImageFormat.Jpeg);


                //store boxes
                StreamWriter _writer = new StreamWriter(TestImagesPath + _imagedata.Name + ".bboxes.tsv");
                //sample ROI will be updated while evaluation goes on
                _writer.WriteLine("0 0 5530 3320");
                _writer.Close();

                //store tsv
                StreamWriter _writerTsv = new StreamWriter(TestImagesPath + _imagedata.Name + ".tsv");
                
                //add dummy tagnames, will be updated once you run the Algorithm
                _writerTsv.WriteLine("drilljigsaw");
                _writerTsv.Close();



                string path2 = "C:\\cntk\\Examples\\Image\\DataSets\\tools\\";
                //store text
                StreamWriter _writer_testimage = new StreamWriter(path2 + "test_img_file.txt");
                _writer_testimage.WriteLine("0\ttestImages/" + _imagedata.Name + ".jpg\t0");
                _writer_testimage.Close();


                return Request.CreateResponse<string>(HttpStatusCode.OK, "Image Uploaded", Configuration.Formatters.JsonFormatter); 
            }
            catch (Exception ex)
            {
                return Request.CreateResponse<string>(HttpStatusCode.BadRequest, "Error Occurred : " + ex.Message + ex.StackTrace, Configuration.Formatters.JsonFormatter);
            }


        }

        [HttpGet]
        [Route("GetRois")]
        public HttpResponseMessage GetRois()
        {

            try
            {
                var pattern = @"\[(.*?)\]";
                string text = System.IO.File.ReadAllText("C:\\Users\\vmadmin\\Desktop\\ROIs.txt");
                var matches = Regex.Matches(text, pattern);

                List<ROIData> _fulldata = new List<ROIData>();


                int matchescoutn = matches.Count;


                if(matchescoutn>0)
                {
                    for (int i = 0; i < matchescoutn/2; i++)
                    {
                        ROIData _temp = new ROIData();


                        string tagline = matches[i + (matchescoutn / 2)].Value.Replace('[', ' ').Replace(']', ' ');
                        var tags = tagline.Split(' ');

                        _temp.TagName = tags[0];
                        _temp.Confidence = tags[1];

                        string coordinates = matches[i].Value.Replace('[', ' ').Replace(']', ' ');
                        var recvalues = coordinates.Split(',');

                        ObjectRectangle rect = new ObjectRectangle();
                        rect.Left = int.Parse(recvalues[0]);
                        rect.Top = int.Parse(recvalues[1]);
                        rect.Height = int.Parse(recvalues[3]);
                        rect.Width = int.Parse(recvalues[2]);

                        _temp.ObjectRect = rect;

                        _fulldata.Add(_temp);
                    }
                }


                return Request.CreateResponse<List<ROIData>>(HttpStatusCode.OK, _fulldata, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse<string>(HttpStatusCode.OK, ex.Message, Configuration.Formatters.JsonFormatter);
            }
           
         
        }

    }

    public class ImageData
    {
        public string Name { get; set; }
        public string Data { get; set; }
    }

    public class ROIData
    {
        public string Confidence { get; set; }
        public string TagName { get; set; }
        public ObjectRectangle ObjectRect { get; set; }
    }

    public class ObjectRectangle
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
    }

}
