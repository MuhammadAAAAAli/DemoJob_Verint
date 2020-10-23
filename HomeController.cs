using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
// ReSharper disable PossibleNullReferenceException

namespace DemoJob.Controllers
{
    public enum MethodNames
    {
        ToLower,
        ToUpper,
        RemoveWhiteSpaces,
        SortByAscii
    }


    public class StringFunctionsMap
    {
        public string ToLower(string input)
        {
            return input.ToLower();
        }

        public string ToUpper(string input)
        {
            return input.ToUpper();
        }

        public string RemoveWhiteSpaces(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => c != ' ')
                .ToArray());
        }

        public string SortByAscii(string input)
        {
            var temp = input.ToCharArray();
            Array.Sort(temp, StringComparer.Ordinal);
            return new string(temp);
        }
    }

    public class HomeController : Controller
    {
        private List<MethodInfo> _userChoosedStage = new List<MethodInfo>();

        [HttpPost]
        public string UploadFile(HttpPostedFileBase file)
        {
            return null;
        }

        public string Index()
        {
            var output = new StringBuilder("  <form id='formid' action=\"/Home/FileUpload\" method=\"POST\" enctype=\"multipart/form-data\"> <input id='fileid' type='file' name='filename' hidden/> <input id='buttonid' type='button' value='Upload input.txt'/> <input type='submit' value='Submit' hidden/> </form> <script>document.getElementById('buttonid').addEventListener('click', openDialog); function openDialog(){document.getElementById('fileid').click();}document.getElementById('fileid').addEventListener('change', submitForm); function submitForm(){document.getElementById('formid').submit();}</script> ");
            output.Append("<hr>METHODS:<br><br>");
            foreach (var name in Enum.GetNames(typeof(MethodNames)))
            {
                output.Append(name + "<br>");
            }

            output.Append("<br><hr><br>");
            output.Append("EXAMPLES :<br><br>");
            output.Append("/Home/UpdateStages?stageNumber=1&addOrRemove=add&exactMethodName=ToLower <br>");
            return output.ToString();
        }

        [HttpGet]
        public string UpdateStages(string stageNumber, string addOrRemove, string exactMethodName)
        {
            int userReqStage;

            try
            {
                userReqStage = int.Parse(stageNumber);
            }
            catch (Exception)
            {
                return "First param is a number from 1 to 3";
            }

            // GENERAL CHECK
            if (!Enum.GetNames(typeof(MethodNames)).Any(n => n == exactMethodName))
            {
                return "No such function name";
            }
            if (userReqStage < 1 || userReqStage > 3)
            {
                return "Only stages 1,2,3";
            }
            if (addOrRemove != "add" && addOrRemove != "remove")
            {
                return "Only add/remove actions";
            }

            if (Session[Request.UserHostAddress + "|" + userReqStage] != null)
                _userChoosedStage = (List<MethodInfo>)Session[Request.UserHostAddress + "|" + userReqStage];
            else
                _userChoosedStage = new List<MethodInfo>();

            // SPECIFIC BASIC CHECKS
            if (_userChoosedStage.Count == 5 && addOrRemove == "add")
            {
                return "Max 5 reached";
            }
            if (_userChoosedStage.Count == 0 && addOrRemove == "remove")
            {
                return "Nothing to remove";
            }


            // DO THE WORK
            if (addOrRemove == "add")
            {
                _userChoosedStage.Add(new StringFunctionsMap().GetType().GetMethod(exactMethodName));
            }
            if (addOrRemove == "remove")
            {
                _userChoosedStage.Remove(new StringFunctionsMap().GetType().GetMethod(exactMethodName));
            }


            Session[Request.UserHostAddress + "|" + userReqStage] = _userChoosedStage;

            return "SUCCES";
        }

        [HttpPost]
        public string FileUpload()
        {
            string fileContent;

            using (var b = new BinaryReader(Request.Files[0].InputStream))
            {
                fileContent = Encoding.UTF8.GetString(b.ReadBytes(Request.Files[0].ContentLength));
            }

            var lines = fileContent.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();

            var allStages = new List<MethodInfo>();

            for (var i = 1; i <= 3; i++)
            {
                if (Session[Request.UserHostAddress + "|" + i] != null)
                    allStages.AddRange((List<MethodInfo>)Session[Request.UserHostAddress + "|" + i]);
            }

            var finalResult = new List<string>();
            if (allStages.Count > 0)
            {
                foreach (var stage in allStages)
                {
                    finalResult = new List<string>();
                    foreach (var line in lines)
                    {
                        finalResult.Add(stage.Invoke(new StringFunctionsMap(), new object[] { line }).ToString());
                    }

                    lines = finalResult;
                }
            }
            else
            {
                finalResult = lines;
            }

            return string.Join("<br>", finalResult.ToArray());
        }
    }
}
