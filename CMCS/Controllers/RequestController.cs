﻿using CMCS.DBContext;
using CMCS.Models;
using Humanizer.DateTimeHumanizeStrategy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Identity;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Security.Cryptography.Xml;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Drawing;
using PdfSharp.UniversalAccessibility.Drawing;

namespace CMCS.Controllers
{
    public class RequestController : Controller
    {
        // Data fields
        private readonly AppDbContext? _context;
        private readonly IConfiguration? _configuration;

        /// <summary>
        /// Master constructor
        /// </summary>
        /// <param name="context"></param>
        public RequestController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// CMCS Status page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Status()
        {
            return View();
        }

        /// <summary>
        /// CMCS NewRequest page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> NewRequest()
        {
            // POST method
            if (this.Request.Method == "POST")
            {
                if (this.Request.Headers["ActionName"] == "NewRequest")
                    await POST_NewRequest_New();

                if (this.Request.Headers["ActionName"] == "EditRequest")
                    await POST_NewRequest_Edit();
            }

            // GET method
            if (this.Request.Method == "GET")
            {
                if (this.Request.Query["ActionName"] == "EditRequest")
                    return await GET_NewRequest_Edit();
            }

            return View();
        }

        /// <summary>
        /// CMCS TrackRequest page.
        /// </summary>
        /// <returns></returns>
        public IActionResult TrackRequest()
        {
            return View();
        }

        /// <summary>
        /// CMCS HR Adminstrative Tools page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> HR_View()
        {
            if (this.Request.Method == "POST")
            {
                if (this.Request.Headers["ActionName"] == "DeleteAccountData")
                    await HR_View_DeleteAccountData();

                if (this.Request.Headers["ActionName"] == "DeleteAccount")
                    await HR_View_DeleteAccount();
            }

            if (this.Request.Method == "GET")
            {
                if (this.Request.Headers["ActionName"] == "GenerateReport")
                    await HR_View_GenerateReport();

                if (this.Request.Headers["ActionName"] == "GetStatistics")
                    await HR_View_GetStatistics();
            }

            return View();
        }

        /// <summary>
        /// This method is part of the 'HR_View' method.
        /// </summary>
        /// <returns></returns>
        private async Task HR_View_GenerateReport()
        {
            // Open the database connection
            await CMCSDB.OpenConnection();

            // Count the number of claims from the database.
            string sql1 = $"SELECT * FROM Request";
            int claimCount = await CMCSDB.CountRows(sql1);

            // Count the number of approved claims from the database.
            string sql2 = $"SELECT * FROM Request WHERE RequestStatus = 'Approved'";
            int approvedClaimCount = await CMCSDB.CountRows(sql2);

            // Count the number of rejected claims from the database.
            string sql3 = $"SELECT * FROM Request WHERE RequestStatus = 'Rejected'";
            int rejectedClaimCount = await CMCSDB.CountRows(sql3);

            // Count the number of pending claims from the database.
            string sql4 = $"SELECT * FROM Request WHERE RequestStatus = 'Pending'";
            int pendingClaimCount = await CMCSDB.CountRows(sql4);

            // Close the database connection.
            await CMCSDB.CloseConnection();

            // Declare and instantiate a PdfDocument object.
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();

            // Page dimensions
            const int PAGE_WIDTH = 595;
            const int PAGE_HEIGHT = 842;

            // Assign the page dimensions to the 'page' object.
            page.Width = new XUnit(PAGE_WIDTH);
            page.Height = new XUnit(PAGE_HEIGHT);

            // Declare and instantiate an XGraphics object.
            XGraphics graphics = XGraphics.FromPdfPage(page);

            // Obtain the logo from the resources.
            string? base64Logo = _configuration?.GetSection("Base64Resources").GetValue<string>("Logo");
            // Convert the logo's content to bytes.
            byte[] logo_bytes = Convert.FromBase64String(base64Logo);

            // Declare and instantiate a MemoryStream object.
            MemoryStream stream = new MemoryStream();
            // Write the content of the logo to the stream.
            stream.Write(logo_bytes, 0, logo_bytes.Length);
            // Declare and instantiate an XImage object from the stream.
            XImage logo = XImage.FromStream(stream);

            // Logo dimensions.
            int logo_dest_width = 150;
            int logo_dest_height = 150;

            // Declarations
            XPen pen1 = new XPen(XColor.FromArgb(0, 0, 0), 1);
            XSolidBrush brush1 = new XSolidBrush(XColor.FromArgb(0, 0, 0));
            XFont heading_font = new XFont("Arial", 20);
            XFont text_font = new XFont("Arial", 12);

            // Drawing operations.
            graphics.DrawImage(logo, (PAGE_WIDTH / 2) - (logo_dest_width / 2), 30, logo_dest_width, logo_dest_height);
            graphics.DrawString("Contract Monthly Claim System", heading_font, brush1, (PAGE_WIDTH / 2) - 140, 220);
            graphics.DrawString("Claim Processing Report", text_font, brush1, (PAGE_WIDTH / 2) - 70, 240);

            graphics.DrawLine(pen1, (PAGE_WIDTH / 2) - 210, 260, (PAGE_WIDTH / 2) + 210, 260);

            graphics.DrawString($"Total number of claims: {claimCount}", text_font, brush1, (PAGE_WIDTH / 2) - 210, 290);
            graphics.DrawString($"Total number of approved claims: {approvedClaimCount}", text_font, brush1, (PAGE_WIDTH / 2) - 210, 310);
            graphics.DrawString($"Total number of rejected claims: {rejectedClaimCount}", text_font, brush1, (PAGE_WIDTH / 2) - 210, 330);
            graphics.DrawString($"Total number of pending claims: {pendingClaimCount}", text_font, brush1, (PAGE_WIDTH / 2) - 210, 350);

            graphics.DrawLine(pen1, (PAGE_WIDTH / 2) - 210, 370, (PAGE_WIDTH / 2) + 210, 370);

            graphics.DrawString($"Date and Time: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", text_font, brush1, (PAGE_WIDTH / 2) - 210, 390);
            graphics.DrawString($"Generated by: {CMCSMain.User.FirstName} {CMCSMain.User.LastName}", text_font, brush1, (PAGE_WIDTH / 2) - 210, 410);

            // Generate output
            MemoryStream outputStream = new MemoryStream();
            // Save the pdf document to the output stream.
            document.Save(outputStream);
            // Close the pdf document.
            document.Close();

            // Convert the content of the output stream to base64 content.
            string base64_content = Convert.ToBase64String(outputStream.ToArray());

            // The request succeeded.
            this.Response.StatusCode = 1;
            await this.Response.WriteAsync(base64_content);
            await this.Response.CompleteAsync();
        }

        private async Task HR_View_DeleteAccountData()
        {
            // Open the database connection.
            await CMCSDB.OpenConnection();

            string? identityNumber = this.Request.Headers["IdentityNumber"];
            int lecturerId = await CMCSDB.FindLecturer(identityNumber);

            if(lecturerId != 0)
            {
                // Declare and instantiate a generic collection.
                List<int> requestList = new List<int>();

                string sql = $"SELECT * FROM Request WHERE LecturerID = {lecturerId}";
                // Create a SqlDataReader object.
                SqlDataReader? reader = await CMCSDB.RunSQLResult(sql);

                // Loop through the result set.
                while(reader != null && reader.Read())
                {
                    string? sRequestID = reader["RequestID"].ToString();
                    int iRequestID = Convert.ToInt32(sRequestID);
                    requestList.Add(iRequestID);
                }

                // Close the SqlDataReader object.
                await CMCSDB.CloseReader();

                // Iterate through the requestList collection.
                for (int i = 0; i < requestList.Count; i++)
                {
                    int requestId = requestList[i];

                    string sql_delete1 = $"DELETE FROM Request WHERE RequestID = {requestId}";
                    string sql_delete2 = $"DELETE FROM RequestDocument WHERE RequestID = {requestId}";
                    string sql_delete3 = $"DELETE FROM RequestProcess WHERE RequestID = {requestId}";

                    // Run SQL queries.
                    await CMCSDB.RunSQLNoResult(sql_delete1);
                    await CMCSDB.RunSQLNoResult(sql_delete2);
                    await CMCSDB.RunSQLNoResult(sql_delete3);
                }

                string sql2 = $"DELETE FROM Document WHERE UserID = '{identityNumber}' AND Section = 'REQUEST'";
                // Run the SQL query.
                await CMCSDB.RunSQLNoResult(sql2);

                // The request succeeded.
                this.Response.StatusCode = 1;
            }
            else
            {
                // The request failed.
                this.Response.StatusCode = 2;
            }

            // Close the database connection.
            await CMCSDB.CloseConnection();
        }

        private async Task HR_View_DeleteAccount()
        {
            // Open the database connection.
            await CMCSDB.OpenConnection();

            string? identityNumber = this.Request.Headers["IdentityNumber"];
            int lecturerId = await CMCSDB.FindLecturer(identityNumber);

            if(lecturerId != 0)
            {
                // Declare and instantiate a generic collection.
                List<int> requestList = new List<int>();

                string sql = $"SELECT * FROM Request WHERE LecturerID = {lecturerId}";
                // Create a SqlDataReader object.
                SqlDataReader? reader = await CMCSDB.RunSQLResult(sql);

                // Loop through the result set.
                while (reader != null && reader.Read())
                {
                    string? sRequestID = reader["RequestID"].ToString();
                    int iRequestID = Convert.ToInt32(sRequestID);
                    requestList.Add(iRequestID);
                }

                // Close the SqlDataReader object.
                await CMCSDB.CloseReader();

                // Iterate through the requestList collection.
                for (int i = 0; i < requestList.Count; i++)
                {
                    int requestId = requestList[i];

                    string sql_delete1 = $"DELETE FROM Request WHERE RequestID = {requestId}";
                    string sql_delete2 = $"DELETE FROM RequestDocument WHERE RequestID = {requestId}";
                    string sql_delete3 = $"DELETE FROM RequestProcess WHERE RequestID = {requestId}";

                    // Run SQL queries.
                    await CMCSDB.RunSQLNoResult(sql_delete1);
                    await CMCSDB.RunSQLNoResult(sql_delete2);
                    await CMCSDB.RunSQLNoResult(sql_delete3);
                }

                string sql2 = $"DELETE FROM Document WHERE UserID = '{identityNumber}'";
                // Run the SQL query.
                await CMCSDB.RunSQLNoResult(sql2);

                string sql3 = $"DELETE FROM Lecturer WHERE LecturerID = {lecturerId}";
                // Run the SQL query.
                await CMCSDB.RunSQLNoResult(sql3);

                // The request succeeded.
                this.Response.StatusCode = 1;
            }
            else
            {
                // The request failed.
                this.Response.StatusCode = 2;
            }

            // Close the database connection.
            await CMCSDB.CloseConnection();
        }

        private async Task HR_View_GetStatistics()
        {
            // Open the database connection.
            await CMCSDB.OpenConnection();

            string sql1 = $"SELECT * FROM Request";
            // Count the number of claims.
            int claimCount = await CMCSDB.CountRows(sql1);

            string sql2 = $"SELECT * FROM Request WHERE RequestStatus = 'Approved'";
            // Count the number of approved claims.
            int approvedClaimCount = await CMCSDB.CountRows(sql2);

            string sql3 = $"SELECT * FROM Request WHERE RequestStatus = 'Rejected'";
            // Count the number of rejected claims.
            int rejectedClaimCount = await CMCSDB.CountRows(sql3);

            string sql4 = $"SELECT * FROM Request WHERE RequestStatus = 'Pending'";
            // Count the number of pending claims.
            int pendingClaimCount = await CMCSDB.CountRows(sql4);

            // Create a dynamic object.
            dynamic statistics = new
            {
                ClaimCount = claimCount,
                ApprovedClaimCount = approvedClaimCount,
                RejectedClaimCount = rejectedClaimCount,
                PendingClaimCount = pendingClaimCount
            };

            // The request succeeded.
            this.Response.StatusCode = 1;
            await this.Response.WriteAsync(JsonConvert.SerializeObject((object?)statistics));
            await this.Response.CompleteAsync();

            // Close the database connection.
            await CMCSDB.CloseConnection();
        }

        /// <summary>
        /// CMCS ManageRequests page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> ManageRequests()
        {
            // Open the database connection.
            await CMCSDB.OpenConnection();

            // POST method
            if (this.Request.Method == "POST")
            {
                if (this.Request.Headers["ActionName"] == "SetRequestIndex")
                    await ManageRequests_SetRequestIndex();

                if (this.Request.Headers["ActionName"] == "AcceptRequest")
                    await ManageRequests_AcceptRequest();

                if (this.Request.Headers["ActionName"] == "RejectRequest")
                    await ManageRequests_RejectRequest();

                if (this.Request.Headers["ActionName"] == "ProcessAllRequests")
                    await ManageRequests_ProcessAllRequests();
            }

            // GET method
            if (this.Request.Method == "GET")
            {
                if (this.Request.Headers["ActionName"] == "GetRequestData")
                    await ManageRequests_GetRequestData();

                if (this.Request.Headers["ActionName"] == "GetDocumentContent")
                    await ManageRequests_GetDocumentContent();

                if (this.Request.Headers["ActionName"] == "GetDocumentContent2")
                    ManageRequests_GetDocumentContent2();

                if (this.Request.Headers["ActionName"] == "GetRequestID")
                    await ManageRequests_GetRequestID();

                if(!this.Request.Headers.ContainsKey("ActionName"))
                {
                    return await GET_ManageRequests();
                }
            }

            // Close the database connection.
            await CMCSDB.CloseConnection();

            return View(new List<Request>());
        }

        /// <summary>
        /// CMCS CancelRequest page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> CancelRequest()
        {
            // POST method
            if (this.Request.Method == "POST")
            {
                if (this.Request.Headers["ActionName"] == "CancelRequest")
                    return await POST_CancelRequest();
            }

            if (!CMCSMain.User.IsManager)
            {
                // Get the request id.
                string? requestID = this.Request.Query["RequestID"].ToString();

                // Open the database connection.
                await CMCSDB.OpenConnection();

                // Find the lecturer and get the id.
                int lecturerID = await CMCSDB.FindLecturer(CMCSMain.User.IdentityNumber);

                // Select all the columns where the request id is provided.
                string sql = $"SELECT * FROM Request WHERE RequestID = {requestID}";
                // Execute the sql query and instantiate a SqlDataReader? object.
                SqlDataReader? reader = await CMCSDB.RunSQLResult(sql);

                if (reader != null)
                {
                    // Check if the reader can read data.
                    if (reader.Read())
                    {
                        // Obtain the lecturer id.
                        int lecturerID2 = Convert.ToInt32(reader["LecturerID"].ToString());

                        // Check if the two values matches.
                        if (lecturerID == lecturerID2)
                        {
                            // Close the SqlDataReader? object.
                            await CMCSDB.CloseReader();
                            // Close the database connection.
                            await CMCSDB.CloseConnection();

                            return View();
                        }
                    }

                    // Close the SqlDataReader? object.
                    await CMCSDB.CloseReader();
                    // Close the database connection.
                    await CMCSDB.CloseConnection();
                }
            }

            // These values will be shown in the address bar.
            RouteValueDictionary routeValues = new RouteValueDictionary();
            routeValues.Add("ActionName", "CancelRequest");
            routeValues.Add("Status", "Failed");

            return RedirectToAction("Status", "Home", routeValues);
        }

        /// <summary>
        /// Gets the request status.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task GetRequestStatus()
        {
            // Obtain the request id from the request query.
            string? requestId = this.Request.Query["RequestID"];

            // Open the database connection.
            await CMCSDB.OpenConnection();

            string sql = $"SELECT RequestStatus FROM Request WHERE RequestID = {requestId}";
            // Declare and instantiate a SqlDataReader? object.
            SqlDataReader? reader = await CMCSDB.RunSQLResult(sql);

            if (reader != null && reader.Read())
            {
                // Get the status of the request.
                string? status = reader["RequestStatus"].ToString();

                // The request succeeded.
                this.Response.StatusCode = 1;
                await this.Response.WriteAsync(status);
                await this.Response.CompleteAsync();
            }
            else
            {
                this.Response.StatusCode = 2;
            }

            // Close the SqlDataReader? object.
            await CMCSDB.CloseReader();
            // Close the database connection.
            await CMCSDB.CloseConnection();
        }

        /// <summary>
        /// Exports the user's requests to a log file.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ExportLog()
        {
            // Declare and instantiate a StringBuilder object.
            StringBuilder sb = new StringBuilder();

            // Obtain the user information.
            string? firstName = CMCSMain.User.FirstName;
            string? lastName = CMCSMain.User.LastName;
            string? identityNumber = CMCSMain.User.IdentityNumber;
            string creationDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            // Build the string output.
            sb.AppendLine($"LOG REPORT");
            sb.AppendLine($"User: {firstName} {lastName}");
            sb.AppendLine($"Identity Number: {identityNumber}");
            sb.AppendLine($"Date Created: {creationDate}");
            sb.AppendLine("-------------------------------------");
            sb.AppendLine();

            // Open the database connection.
            await CMCSDB.OpenConnection();

            // Obtain the lecturer id.
            int lecturerId = await CMCSDB.FindLecturer(CMCSMain.User.IdentityNumber);
            string sql = $"SELECT * FROM Request r INNER JOIN RequestProcess p ON r.RequestID = p.RequestID WHERE LecturerID = {lecturerId}";
            // Declare and instantiate a SqlDataReader? object.
            SqlDataReader? reader = await CMCSDB.RunSQLResult(sql);

            if (reader != null)
            {
                sb.AppendLine("Accepted/Rejected Request(s):");

                // Iterate while the SqlDataReader? object can read data.
                while (reader.Read())
                {
                    // Obtain the data from the SqlDataReader? object.
                    string requestID = reader["RequestID"].ToString();
                    string requestFor = reader["RequestFor"].ToString();
                    string hoursWorked = reader["HoursWorked"].ToString();
                    string hourlyRate = reader["HourlyRate"].ToString();
                    string description = reader["Description"].ToString();
                    string requestStatus = reader["RequestStatus"].ToString();
                    string dateSubmitted = DateTime.Parse(reader["DateSubmitted"].ToString()).ToString("dd/MM/yyyy HH:mm:ss");
                    string date = DateTime.Parse(reader["Date"].ToString()).ToString("dd/MM/yyyy HH:mm:ss");

                    sb.AppendLine($"Request ID: {requestID}; Request For: {requestFor}; Hours Worked: {hoursWorked}; Hourly Rate: {hourlyRate}; Description: {description}; Date Submitted: {dateSubmitted}; Date Approved/Rejected: {date}");
                }

                // Close the SqlDataReader? object.
                await CMCSDB.CloseReader();

                string sql2 = $"SELECT * FROM Request WHERE LecturerID = {lecturerId} AND RequestStatus = 'Pending'";
                // Re-instantiate a SqlDataReader? object.
                reader = await CMCSDB.RunSQLResult(sql2);

                sb.AppendLine();
                sb.AppendLine("Pending Request(s):");

                // Iterate while the SqlDataReader? object can read data.
                while (reader.Read())
                {
                    // Obtain the data from the SqlDataReader? object.
                    string? requestID = reader["RequestID"].ToString();
                    string? requestFor = reader["RequestFor"].ToString();
                    string? hoursWorked = reader["HoursWorked"].ToString();
                    string? hourlyRate = reader["HourlyRate"].ToString();
                    string? description = reader["Description"].ToString();
                    string? requestStatus = reader["RequestStatus"].ToString();
                    string? dateSubmitted = DateTime.Parse(reader["DateSubmitted"].ToString()).ToString("dd/MM/yyyy HH:mm:ss");

                    sb.AppendLine($"Request ID: {requestID}; Request For: {requestFor}; Hours Worked: {hoursWorked}; Hourly Rate: {hourlyRate}; Description: {description}; Date Submitted: {dateSubmitted}");
                }


                // The request succeeded.
                this.Response.StatusCode = 1;
                this.Response.Headers.Add("FileName", $"LOG-{Guid.NewGuid().ToString().ToUpper()}.txt");
                await this.Response.WriteAsync(sb.ToString());
                await this.Response.CompleteAsync();
            }
            else
            {
                this.Response.StatusCode = 2;
            }

            // Close the SqlDataReader? object.
            await CMCSDB.CloseReader();
            // Close the database connection.
            await CMCSDB.CloseConnection();

            return View();
        }

        /// <summary>
        /// This method is part of the NewRequest method.
        /// </summary>
        private async Task POST_NewRequest_New()
        {
            // Read the content from the request body, asynchronously.
            var sRequestData = await new StreamReader(this.Request.Body).ReadToEndAsync();
            // Parse the string content into a dynamic object, using JsonConvert.
            dynamic? requestData = JsonConvert.DeserializeObject(sRequestData);

            // Get the fields from the dynamic object as strings.
            string? requestFor = Convert.ToString(requestData?.RequestFor);
            string? hoursWorked = Convert.ToString(requestData?.HoursWorked);
            string? hourlyRate = Convert.ToString(requestData?.HourlyRate);
            string? description = Convert.ToString(requestData?.Description);

            dynamic? documents = requestData?.Documents;

            // Open the database connection.
            await CMCSDB.OpenConnection();

            // Get the lecturerId from an IdentityNumber.
            int lecturerId = await CMCSDB.FindLecturer(CMCSMain.User.IdentityNumber);

            string sql2 = $"INSERT INTO Request(LecturerID, RequestFor, HoursWorked, HourlyRate, Description, RequestStatus, DateSubmitted) OUTPUT INSERTED.RequestID VALUES ('{lecturerId}', '{requestFor}', {hoursWorked}, {hourlyRate}, '{description}', 'Pending', '{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}')";
            // Run the sql query and obtain the output.
            int? requestID = (int?)await CMCSDB.RunSQLResultScalar(sql2);

            // Iterate through the 'documents' collection.
            for (int i = 0; i < documents?.Count; i++)
            {
                // Get the field values from the dynamic object 'dpcuments'.
                string documentName = Convert.ToString(documents[i]?.Name);
                string documentType = CMCSMain.GetDocumentType(documentName);

                string sql1 = $"INSERT INTO Document(Name, Type, Size, Section, UserID, Content) OUTPUT INSERTED.DocumentID VALUES ('{documents?[i].Name}', '{documentType}', {documents?[i].Size}, 'REQUEST', '{CMCSMain.User.IdentityNumber}', '{documents?[i].Content}')";
                // Run the sql query and obtain the output.
                int? documentID = (int?)await CMCSDB.RunSQLResultScalar(sql1);

                string sql3 = $"INSERT INTO RequestDocument(RequestID, DocumentID) VALUES ({requestID}, {documentID})";
                // Run the sql query and do not return an output value.
                await CMCSDB.RunSQLNoResult(sql3);
            }

            // Close the database connection.
            await CMCSDB.CloseConnection();

            // The request succeeded.
            this.Response.StatusCode = 1;
        }

        /// <summary>
        /// This method is part of the CancelRequest method.
        /// </summary>
        /// <returns></returns>
        private async Task<IActionResult> POST_CancelRequest()
        {
            // Obtain the request id from the request header collection.
            string? requestId = this.Request.Headers["RequestID"].ToString();

            // Open the database connection.
            await CMCSDB.OpenConnection();

            string sql4 = $"SELECT * FROM RequestDocument WHERE RequestID = {requestId}";
            // Create a SqlDataReader? object from a sql query.
            SqlDataReader? reader = await CMCSDB.RunSQLResult(sql4);

            if (reader != null)
            {
                // Declare and instantiate a List<int> object.
                List<int> documentList = new List<int>();

                // Iterate through the collection.
                while (reader.Read())
                {
                    int documentId = Convert.ToInt32(reader["DocumentID"].ToString());
                    documentList.Add(documentId);
                }

                // Close the SqlDataReader?.
                await CMCSDB.CloseReader();

                // Iterate through the collection.
                foreach (int documentId in documentList)
                {
                    string sql5 = $"DELETE FROM Document WHERE DocumentID = {documentId}";
                    await CMCSDB.RunSQLNoResult(sql5);
                }

                // Delete all the data from the Request, RequestDocument and RequestProcess tables.
                string sql1 = $"DELETE FROM Request WHERE RequestID = {requestId}";
                string sql2 = $"DELETE FROM RequestDocument WHERE RequestID = {requestId}";
                string sql3 = $"DELETE FROM RequestProcess WHERE RequestID = {requestId}";

                // Run the sql queries.
                await CMCSDB.RunSQLNoResult(sql1);
                await CMCSDB.RunSQLNoResult(sql2);
                await CMCSDB.RunSQLNoResult(sql3);
                // The request succeeded.
                this.Response.StatusCode = 1;
            }
            else
            {
                this.Response.StatusCode = 2;
            }

            // Close the database connection.
            await CMCSDB.CloseConnection();

            return View();
        }

        /// <summary>
        /// This method is part of the NewRequest method.
        /// </summary>
        private async Task POST_NewRequest_Edit()
        {
            // Obtain the request id from the request query.
            string? sRequestID = this.Request.Headers["RequestID"];

            // Read the data from the request body, asynchronously.
            var sRequestData = await new StreamReader(this.Request.Body).ReadToEndAsync();
            // Convert the string to a dynamic object using JsonConvert.
            dynamic? requestData = JsonConvert.DeserializeObject(sRequestData);

            // Obtain the field values from the dynamic object 'requestData'.
            string? requestFor = Convert.ToString(requestData?.RequestFor);
            string? hoursWorked = Convert.ToString(requestData?.HoursWorked);
            string? hourlyRate = Convert.ToString(requestData?.HourlyRate);
            string? description = Convert.ToString(requestData?.Description);

            dynamic? documents = requestData?.Documents;
            dynamic? deletedDocuments = requestData?.DeletedDocuments;

            // Open the database connection.
            await CMCSDB.OpenConnection();

            string sql2 = $"UPDATE Request SET RequestFor = '{requestFor}', HoursWorked = {hoursWorked}, HourlyRate = {hourlyRate}, Description = '{description}' WHERE RequestID = {sRequestID}";
            await CMCSDB.RunSQLNoResult(sql2);

            // Iterate through the collection.
            for (int i = 0; i < documents?.Count; i++)
            {
                string documentName = Convert.ToString(documents[i]?.Name);
                string documentType = CMCSMain.GetDocumentType(documentName);

                string sql1 = $"INSERT INTO Document(Name, Type, Size, Section, UserID, Content) OUTPUT INSERTED.DocumentID VALUES ('{documents?[i].Name}', '{documentType}', {documents?[i].Size}, 'REQUEST', '{CMCSMain.User.IdentityNumber}', '{documents?[i].Content}')";
                int? documentID = (int?)await CMCSDB.RunSQLResultScalar(sql1);

                string sql3 = $"INSERT INTO RequestDocument(RequestID, DocumentID) VALUES ({sRequestID}, {documentID})";
                await CMCSDB.RunSQLNoResult(sql3);
            }

            // Check if the variable 'deletedDocuments' does not equal 'DELETE_ALL'.
            if (Convert.ToString(deletedDocuments) != "DELETE_ALL")
            {
                // Iterate through the collection.
                for (int i = 0; i < deletedDocuments?.Count; i++)
                {
                    int documentId = Convert.ToInt32(deletedDocuments[i]);

                    string sql1 = $"DELETE FROM RequestDocument WHERE DocumentID = {documentId}";
                    await CMCSDB.RunSQLNoResult(sql1);

                    string sql3 = $"DELETE FROM Document WHERE DocumentID = {documentId}";
                    await CMCSDB.RunSQLNoResult(sql3);
                }

                // The request succeeded.
                this.Response.StatusCode = 1;
            }
            else
            {
                // Variable declarations.
                string sql = $"SELECT * FROM RequestDocument WHERE RequestID = {sRequestID}";
                int row_count = await CMCSDB.CountRows(sql);
                var reader = await CMCSDB.RunSQLResult(sql);

                int[] documentIDList = new int[row_count];

                if (reader != null)
                {
                    // Iterate through the collection.
                    for (int i = 0; i < row_count; i++)
                    {
                        reader.Read();
                        documentIDList[i] = Convert.ToInt32(reader["DocumentID"].ToString());
                    }
                }

                // Close the SqlDataReader? object.
                await CMCSDB.CloseReader();

                // Iterate through the collection.
                for (int i = 0; i < documentIDList.Length; i++)
                {
                    string sql3 = $"DELETE FROM Document WHERE DocumentID = {documentIDList[i]}";
                    await CMCSDB.RunSQLNoResult(sql3);
                }

                string sql4 = $"DELETE FROM RequestDocument WHERE RequestID = {sRequestID}";
                await CMCSDB.RunSQLNoResult(sql4);

                if(reader != null)
                {
                    // The request succeeded.
                    this.Response.StatusCode = 1;
                }
                else
                {
                    this.Response.StatusCode = 2;
                }
            }

            // Close the database connection.
            await CMCSDB.CloseConnection();
        }

        /// <summary>
        /// This method is part of the NewRequest method.
        /// </summary>
        /// <returns></returns>
        private async Task<IActionResult> GET_NewRequest_Edit()
        {
            // Open the database connection.
            await CMCSDB.OpenConnection();

            // Get the request id from the request query.
            string? sRequestID = this.Request.Query["RequestID"];

            string sql4 = $"SELECT * FROM Request WHERE RequestID = {sRequestID}";
            // Declare and instantiate a SqlDataReader? object.
            SqlDataReader? reader = await CMCSDB.RunSQLResult(sql4);
            reader.Read();

            string? sRequestFor = reader["RequestFor"].ToString();
            string? sHoursWorked = reader["HoursWorked"].ToString();
            string? sHourlyRate = reader["HourlyRate"].ToString();
            string? sDescription = reader["Description"].ToString();

            // Close the SqlDataReader? object.
            await CMCSDB.CloseReader();

            string sql = $"SELECT * FROM RequestDocument r INNER JOIN Document d ON r.DocumentID = d.DocumentID WHERE r.RequestID = {sRequestID}";
            int row_count = await CMCSDB.CountRows(sql);
            reader = await CMCSDB.RunSQLResult(sql);

            // Declare and instantiate a List<CMCSDocument> object.
            List<Document> documents = new List<Document>();

            if (reader != null)
            {
                // Iterate through the collection.
                for (int i = 0; i < row_count; i++)
                {
                    reader.Read();

                    documents.Add(new Document
                    {
                        DocumentID = Convert.ToInt32(reader["DocumentID"].ToString()),
                        Name = reader["Name"].ToString(),
                        Size = Convert.ToInt32(reader["Size"].ToString()),
                        Type = reader["Type"].ToString(),
                        Section = reader["Section"].ToString(),
                        Content = reader["Content"].ToString()
                    });
                }
            }

            // Close the SqlDataReader? object.
            await CMCSDB.CloseReader();
            // Close the database connection.
            await CMCSDB.CloseConnection();

            var view = View(documents);
            view.ViewData["RequestID"] = sRequestID;
            view.ViewData["RequestFor"] = sRequestFor;
            view.ViewData["HoursWorked"] = sHoursWorked;
            view.ViewData["HourlyRate"] = sHourlyRate;
            view.ViewData["Description"] = sDescription;

            return view;
        }

        /// <summary>
        /// This method is part of the ManageRequests method.
        /// </summary>
        private async Task ManageRequests_SetRequestIndex()
        {
            // Set the selected request index.
            CMCSMain.SelectedRequestIndex = Convert.ToInt32(this.Request.Headers["RowSelected"]);

            var reader = await CMCSDB.RunSQLResult($"SELECT * FROM Request");

            if (reader != null)
            {
                // Iterate through the collection.
                for (int i = 0; i < CMCSMain.SelectedRequestIndex; i++)
                    reader.Read();

                // Read data from the SqlDataReader? object.
                reader.Read();
                CMCSMain.SelectedRequestID = Convert.ToInt32(reader["RequestID"].ToString());

                // The request succeeded.
                this.Response.StatusCode = 1;
            }
            else
            {
                this.Response.StatusCode = 2;
            }

            // Close the SqlDataReader? object.
            await CMCSDB.CloseReader();
        }

        /// <summary>
        /// This method is part of the ManageRequests method.
        /// </summary>
        private async Task ManageRequests_GetRequestID()
        {
            // Obtain the request index from the request header 'RequestIndex'.
            int requestIndex = Convert.ToInt32(this.Request.Headers["RequestIndex"]);
            var reader = await CMCSDB.RunSQLResult("SELECT * FROM Request");

            if (reader != null)
            {
                // Iterate through the collection.
                for (int i = 0; i < requestIndex; i++)
                    reader.Read();

                // Read data from the SqlDataReader? object.
                reader.Read();
                int requestID = Convert.ToInt32(reader["RequestID"].ToString());

                // The request succeeded.
                this.Response.StatusCode = 1;
                await this.Response.WriteAsync(requestID.ToString());
                await this.Response.CompleteAsync();
            }
            else
            {
                this.Response.StatusCode = 2;
            }

            // Close the SqlDataReader? object.
            await CMCSDB.CloseReader();
        }

        /// <summary>
        /// This method is part of the ManageRequests method.
        /// </summary>
        private async Task ManageRequests_AcceptRequest()
        {
            // Get the request id from the request header 'RequestID'.
            int requestID = Convert.ToInt32(this.Request.Headers["RequestID"].ToString());

            string sql = $"UPDATE Request SET RequestStatus = 'Approved' WHERE RequestID = {requestID}";
            await CMCSDB.RunSQLNoResult(sql);

            int managerID = (await CMCSDB.FindManager(CMCSMain.User.IdentityNumber));
            string sql2 = $"INSERT INTO RequestProcess(RequestID, ManagerID, Date, Status) VALUES ({requestID}, {managerID}, '{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}', 'Approved')";
            await CMCSDB.RunSQLNoResult(sql2);

            // The request succeeded.
            this.Response.StatusCode = 1;
        }

        /// <summary>
        /// This method is part of the ManageRequests method.
        /// </summary>
        private async Task ManageRequests_RejectRequest()
        {
            // Get the request id from the request header 'RequestID'.
            int requestID = Convert.ToInt32(this.Request.Headers["RequestID"].ToString());

            string sql = $"UPDATE Request SET RequestStatus = 'Rejected' WHERE RequestID = {requestID}";
            await CMCSDB.RunSQLNoResult(sql);

            int managerID = await CMCSDB.FindManager(CMCSMain.User.IdentityNumber);
            string sql2 = $"INSERT INTO RequestProcess(RequestID, ManagerID, Date, Status) VALUES ({requestID}, {managerID}, '{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}', 'Rejected')";
            await CMCSDB.RunSQLNoResult(sql2);

            // The request succeeded.
            this.Response.StatusCode = 1;
        }

        /// <summary>
        /// This method is part of the ManageRequests method.
        /// </summary>
        private async Task ManageRequests_GetRequestData()
        {
            // Get the request id from the request header 'RequestID'.
            int requestID = Convert.ToInt32(this.Request.Headers["RequestID"]);
            string sql2 = $"SELECT * FROM Request r INNER JOIN Lecturer l ON r.LecturerID = l.LecturerID WHERE RequestID = {requestID}";
            SqlDataReader? reader = await CMCSDB.RunSQLResult(sql2);

            if (reader != null)
            {
                // Check if the SqlDataReader? object can read data.
                if (reader.Read())
                {
                    // Obtain the first name and last name from the SqlDataReader? object.
                    string firstName = reader["FirstName"].ToString();
                    string lastName = reader["LastName"].ToString();

                    // Add new response headers to carry the information.
                    this.Response.Headers.Add("RequestID", requestID.ToString());
                    this.Response.Headers.Add("LecturerID", reader["LecturerID"].ToString());
                    this.Response.Headers.Add("IdentityNumber", reader["IdentityNumber"].ToString());
                    this.Response.Headers.Add("Lecturer", $"{firstName} {lastName}");
                    this.Response.Headers.Add("RequestFor", reader["RequestFor"].ToString());
                    this.Response.Headers.Add("HoursWorked", reader["HoursWorked"].ToString());
                    this.Response.Headers.Add("HourlyRate", reader["HourlyRate"].ToString());
                    this.Response.Headers.Add("Description", reader["Description"].ToString());
                }

                // Close the SqlDataReader? object.
                await CMCSDB.CloseReader();

                string sql = $"SELECT * FROM RequestDocument r INNER JOIN Document d ON r.DocumentID = d.DocumentID WHERE r.RequestID = {requestID}";
                int row_count = await CMCSDB.CountRows(sql);
                reader = await CMCSDB.RunSQLResult(sql);

                // Declare and instantiate a List<CMCSDocument> object.
                List<Document> documentList = new List<Document>();

                if (reader != null)
                {
                    // Iterate through the collection.
                    for (int i = 0; i < row_count; i++)
                    {
                        // Read data from the SqlDataReader? object.
                        reader.Read();

                        var document = new Document();
                        document.DocumentID = Convert.ToInt32(reader["DocumentID"]);
                        document.Name = reader["Name"].ToString();
                        document.Size = Convert.ToInt32(reader["Size"]);
                        document.Type = reader["Type"].ToString();
                        document.Section = reader["Section"].ToString();
                        document.Content = reader["Content"].ToString();

                        documentList.Add(document);
                    }
                }

                // Close the SqlDataReader? object.
                await CMCSDB.CloseReader();

                // The request succeeded.
                this.Response.StatusCode = 1;
                await this.Response.WriteAsync(JsonConvert.SerializeObject(documentList.ToArray()));
                await this.Response.CompleteAsync();
            }
            else
            {
                this.Response.StatusCode = 2;
            }
        }

        /// <summary>
        /// This method is part of the ManageRequests method.
        /// </summary>
        private async Task ManageRequests_GetDocumentContent()
        {
            // Obtain the data from the request headers 'RequestID' and 'FileIndex'.
            int requestID = Convert.ToInt32(this.Request.Headers["RequestID"]);
            int fileIndex = Convert.ToInt32(this.Request.Headers["FileIndex"]);

            string sql = $"SELECT * FROM RequestDocument r INNER JOIN Document d ON r.DocumentID = d.DocumentID WHERE r.RequestID = {requestID}";
            // Declare and instantiate a SqlDataReader? object.
            SqlDataReader? reader = await CMCSDB.RunSQLResult(sql);

            if (reader != null)
            {
                // Iterate through the collection.
                for (int i = 0; i < fileIndex; i++)
                    reader.Read();

                // Read data from the SqlDataReader? object.
                reader.Read();
                string filename = reader["Name"].ToString();
                string documentContent = reader["Content"].ToString();

                // The request succeeded.
                this.Response.StatusCode = 1;
                await this.Response.WriteAsync($"[{filename}]{documentContent}");
                await this.Response.CompleteAsync();
            }
            else
            {
                this.Response.StatusCode = 2;
            }

            // Close the SqlDataReader? object.
            await CMCSDB.CloseReader();
        }

        private void ManageRequests_GetDocumentContent2()
        {
            int documentId = Convert.ToInt32(this.Request.Headers["DocumentID"]);
            Document document = _context.Document.Where(i => i.DocumentID == documentId).ToList()[0];

            this.Response.Headers.Add("FileName", document.Name);

            // The request succeeded.
            this.Response.StatusCode = 1;
            this.Response.WriteAsync(document.Content);
            this.Response.CompleteAsync();
        }

        /// <summary>
        /// This method is part of the ManageRequests method.
        /// </summary>
        private async Task<IActionResult> GET_ManageRequests()
        {
            string sql1 = $"SELECT * FROM Request";
            int row_count = await CMCSDB.CountRows(sql1);
            SqlDataReader? reader = await CMCSDB.RunSQLResult(sql1);

            // Declare and instantiate a List<CMCDRequest> object.
            List<Request> requestList = new List<Request>();

            if (reader != null)
            {
                // Iterate through the collection.
                for (int i = 0; i < row_count; i++)
                {
                    // Read data from the SqlDataReader? object.
                    reader.Read();

                    // Declare and instantiate a CMCSRequest object.
                    var request = new Request();
                    request.RequestID = Convert.ToInt32(reader["RequestID"].ToString());
                    request.LecturerID = Convert.ToInt32(reader["LecturerID"].ToString());
                    request.RequestFor = reader["RequestFor"].ToString();
                    request.HoursWorked = Convert.ToInt32(reader["HoursWorked"].ToString());
                    request.HourlyRate = Convert.ToInt32(reader["HourlyRate"].ToString());
                    request.Description = reader["Description"].ToString();
                    request.DateSubmitted = DateTime.Parse(reader["DateSubmitted"].ToString());
                    request.RequestStatus = reader["RequestStatus"].ToString();

                    // Add the request object to the collection 'requestList'.
                    requestList.Add(request);
                }
            }

            // Close the SqlDataReader? object.
            await CMCSDB.CloseReader();

            string sql2 = $"SELECT * FROM Request r INNER JOIN RequestProcess p ON r.RequestID = p.RequestID";
            int row_count2 = await CMCSDB.CountRows(sql2);
            reader = await CMCSDB.RunSQLResult(sql2);

            if (reader != null)
            {
                // Iterate through the collection.
                for (int i = 0; i < row_count2; i++)
                {
                    // Read data from the SqlDataReader? object.
                    reader.Read();

                    // Get the request id from the reader.
                    int requestID = Convert.ToInt32(reader["RequestID"].ToString());

                    // Inner For Loop: Iterate through the requetList collection.
                    for (int j = 0; j < requestList.Count; j++)
                    {
                        // Check if there is a match.
                        if (requestList[j].RequestID == requestID)
                        {
                            requestList[j].DateApproved = DateTime.Parse(reader["Date"].ToString());
                            requestList[j].RequestStatus = reader["Status"].ToString();
                        }
                    }
                }
            }

            // Close the SqlDataReader? object.
            await CMCSDB.CloseReader();

            // Declare and instantiate a generic collection.
            List<string> lecturerNames = new List<string>();

            // Iterate through the collection.
            for (int i = 0; i < requestList.Count; i++)
            {
                string sql3 = $"SELECT * FROM Lecturer WHERE LecturerID = {requestList[i].LecturerID}";
                reader = await CMCSDB.RunSQLResult(sql3);

                if (reader != null && reader.Read())
                {
                    string lecturerName = $"{reader["FirstName"]} {reader["LastName"]}";

                    // Add the result to the 'lecturerNames' collection.
                    lecturerNames.Add(lecturerName);
                }

                // Close the reader.
                await CMCSDB.CloseReader();
            }

            var view = View(requestList);
            view.ViewData["LecturerNames"] = lecturerNames.ToArray();

            return view;
        }


        /// <summary>
        /// This method is part of the 'ManageRequests' method.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        private async Task ManageRequests_ProcessAllRequests()
        {
            string sql = "SELECT * FROM Request";
            // Create a SqlDataReader object.
            SqlDataReader? reader = await CMCSDB.RunSQLResult(sql);
            // Declare and instantiate a generic collection of type 'int'.
            List<int> approvedRequests = new List<int>();

            // Loop through the result set.
            while(reader != null && reader.Read())
            {
                string? sHoursWorked = reader["HoursWorked"].ToString();
                string? sHourlyRate = reader["HourlyRate"].ToString();

                double hoursWorked = Convert.ToDouble(sHoursWorked);
                double hourlyRate = Convert.ToDouble(sHourlyRate);

                // Check if the hours worked and hourly rate matches the predefined criteria.
                if(hoursWorked >= 6 && hourlyRate >= 300)
                {
                    int requestID = Convert.ToInt32(reader["RequestID"].ToString());
                    // Add the requestID to the approvedRequests collection.
                    approvedRequests.Add(requestID);
                }
            }

            // Close the SqlDataReader object.
            await CMCSDB.CloseReader();

            // Iterate through the approvedRequests collection.
            for(int i = 0; i < approvedRequests.Count; i++)
            {
                int requestID = approvedRequests[i];

                string sql2 = $"UPDATE Request SET RequestStatus = 'Approved' WHERE RequestID = {requestID}";
                // Run the SQL query.
                await CMCSDB.RunSQLNoResult(sql2);

                int managerID = (await CMCSDB.FindManager(CMCSMain.User.IdentityNumber));
                string sql3 = $"INSERT INTO RequestProcess(RequestID, ManagerID, Date, Status) VALUES ({requestID}, {managerID}, '{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}', 'Approved')";
                // Run the SQL query.
                await CMCSDB.RunSQLNoResult(sql3);
            }

            // The request succeeded.
            this.Response.StatusCode = 1;
        }
    }
}
