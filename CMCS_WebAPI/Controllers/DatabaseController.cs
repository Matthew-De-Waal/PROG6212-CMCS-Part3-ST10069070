using CMCS_WebAPI.DBContext;
using CMCS_WebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CMCS_WebAPI.Controllers
{
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        // Data fields
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// Master constructor
        /// </summary>
        /// <param name="dbContext"></param>
        public DatabaseController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ------------------ HTTP GET ------------------

        /// <summary>
        /// Gets the lecturers within the CMCS database.
        /// </summary>
        /// <returns></returns>
        [Route("api/Database/GetLecturers")]
        [HttpGet]
        public ActionResult<Lecturer[]> GetLecturers()
        {
            return _dbContext.Lecturer.ToArray();
        }

        /// <summary>
        /// Gets the managers within the CMCS database.
        /// </summary>
        /// <returns></returns>
        [Route("api/Database/GetManagers")]
        [HttpGet]
        public ActionResult<Manager[]> GetManagers()
        {
            return _dbContext.Manager.ToArray();
        }

        /// <summary>
        /// Gets the documents within the CMCS database.
        /// </summary>
        /// <returns></returns>
        [Route("api/Database/GetDocuments")]
        [HttpGet]
        public ActionResult<Document[]> GetDocuments()
        {
            return _dbContext.Document.ToArray();
        }

        /// <summary>
        /// Gets the requests within the CMCS database.
        /// </summary>
        /// <returns></returns>
        [Route("api/Database/GetRequests")]
        [HttpGet]
        public ActionResult<Request[]> GetRequests()
        {
            return _dbContext.Request.ToArray();
        }

        /// <summary>
        /// Gets the request documents within the CMCS database.
        /// </summary>
        /// <returns></returns>
        [Route("api/Database/GetRequestDocuments")]
        [HttpGet]
        public ActionResult<RequestDocument[]> GetRequestDocuments()
        {
            return _dbContext.RequestDocument.ToArray();
        }

        /// <summary>
        /// Gets the request processes within the CMCS database.
        /// </summary>
        /// <returns></returns>
        [Route("api/Database/GetRequestProcesses")]
        [HttpGet]
        public ActionResult<RequestProcess[]> GetRequestProcesses()
        {
            return _dbContext.RequestProcess.ToArray();
        }

        // ------------------ HTTP POST ------------------

        /// <summary>
        /// Adds a recovery method to a lecturer or manager account.
        /// </summary>
        /// <param name="type">Can be either 'FILE' or 'QUESTION'</param>
        /// <param name="identityNumber">The identity number of the lecturer or manager</param>
        /// <param name="value">
        /// If the recovery type is 'FILE' then the value is the recovery key.
        /// If the recovery type is 'QUESTION' then the value is the security question and the security answer
        /// separated by a semicolon. For example, the security question is 'What is my favourite food?' and
        /// the security answer is 'Chicken and Chips'. The value will be 'What is my favourite food?;Chicken and Chips'.
        /// </param>
        /// <returns></returns>
        [Route("api/Database/AddRecoveryMethod")]
        [HttpPost]
        public async Task<ActionResult> AddRecoveryMethod(string type, string identityNumber, string value)
        {
            // Check if the recovery method is 'FILE'.
            if (type == "FILE")
            {
                // Declare and instantiate an AccountRecovery object.
                AccountRecovery recovery = new AccountRecovery();
                recovery.Method = "FILE";
                recovery.UserID = identityNumber;
                recovery.Value = value;

                // Add the data to the AccountRecovery table.
                _dbContext.AccountRecovery.Add(recovery);
                // Save the changes asynchronously.
                await _dbContext.SaveChangesAsync();

                return Ok();
            }

            // Check if the recovery method is 'QUESTION'.
            if (type == "QUESTION")
            {
                // Declare and instantiate an AccountRecovery object.
                AccountRecovery recovery = new AccountRecovery();
                recovery.Method = "QUESTION";
                recovery.UserID = identityNumber;
                recovery.Value = value;

                // Add the data to the AccountRecovery table.
                _dbContext.AccountRecovery.Add(recovery);
                // Save the changes asynchronously.
                await _dbContext.SaveChangesAsync();

                // The operation succeeded.
                return Ok();
            }

            // The operation failed.
            return BadRequest();
        }

        /// <summary>
        /// Removes a recovery method from a lecturer or manager account
        /// </summary>
        /// <param name="type"></param>
        /// <param name="identityNumber"></param>
        /// <returns></returns>
        [Route("api/Database/RemoveRecoveryMethod")]
        [HttpPost]
        public async Task<ActionResult> RemoveRecoveryMethod(string type, string identityNumber)
        {
            // Declare an AccountRecovery object.
            AccountRecovery? recovery = null;
            // Read data from the AccountRecovery table.
            AccountRecovery[] collection = _dbContext.AccountRecovery.Where(i => i.Method == type && i.UserID == identityNumber).ToArray();

            // Check if the size of the collection is zero.
            if(collection.Length == 0)
            {
                // The operation failed.
                return BadRequest();
            }

            // Get the first element from the collection.
            recovery = collection[0];

            // Remove the data from the AccountRecovery table.
            _dbContext.AccountRecovery.Remove(recovery);
            // Save the changes asynchronously.
            await _dbContext.SaveChangesAsync();

            // The operation succeeded.
            return Ok();
        }

        /// <summary>
        /// Approves a claim.
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        [Route("api/Database/ApproveRequest")]
        [HttpPost]
        public async Task<ActionResult> ApproveRequest(int requestId)
        {
            // Variable Declarations
            int managerID = 0;
            Manager[] managers = _dbContext.Manager.ToArray();

            // Check if the size of the array is greater than zero.
            if (managers.Length > 0)
            {
                managerID = managers[0].ManagerID;
            }
            else
                // The operation failed.
                return BadRequest();

            // Create a Request object.
            Request request = _dbContext.Request.Where(i => i.RequestID == requestId).ToArray()[0];
            request.RequestStatus = "Approved";

            // Update the data within the Request table.
            _dbContext.Request.Update(request);
            // Save the changes asynchronously.
            await _dbContext.SaveChangesAsync();

            // Declare and instantiate a RequestProcess object.
            RequestProcess process = new RequestProcess();
            process.RequestID = request.RequestID;
            process.ManagerID = managerID;
            process.Status = "Approved";
            process.Date = DateTime.Now;

            // Add the RequestProcess object to the RequestProcess table.
            _dbContext.RequestProcess.Add(process);
            // Save the changes asynchronously.
            await _dbContext.SaveChangesAsync();

            // The operation succeeded.
            return Ok();
        }

        /// <summary>
        /// Rejects a claim
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        [Route("api/Database/RejectRequest")]
        [HttpPost]
        public async Task<ActionResult> RejectRequest(int requestId)
        {
            // Variable Declarations
            int managerID = 0;
            Manager[] managers = _dbContext.Manager.ToArray();

            // Check if the size of the array is greater than zero.
            if (managers.Length > 0)
            {
                managerID = managers[0].ManagerID;
            }
            else
                // The operation failed.
                return BadRequest();

            // Create a Request object.
            Request request = _dbContext.Request.Where(i => i.RequestID == requestId).ToArray()[0];
            request.RequestStatus = "Rejected";

            // Update the data within the Request table.
            _dbContext.Request.Update(request);
            // Save the changes asynchronously.
            await _dbContext.SaveChangesAsync();

            // Declare and instantiate a RequestProcess object.
            RequestProcess process = new RequestProcess();
            process.RequestID = request.RequestID;
            process.ManagerID = managerID;
            process.Status = "Rejected";
            process.Date = DateTime.Now;

            // Add the RequestProcess object to the RequestProcess table.
            _dbContext.RequestProcess.Add(process);
            // Save the changes asynchronously.
            await _dbContext.SaveChangesAsync();

            // The operation succeeded.
            return Ok();
        }

        /// <summary>
        /// Processes all the claims against a predefined criteria, using the automated system.
        /// </summary>
        /// <returns></returns>
        [Route("api/Database/ProcessAllRequests")]
        [HttpPost]
        public async Task<ActionResult> ProcessAllRequests()
        {
            // Variable Declarations
            int managerID = 0;
            Manager[] managers = _dbContext.Manager.ToArray();

            // Check if the size of the array is greater than zero.
            if (managers.Length > 0)
            {
                managerID = managers[0].ManagerID;
            }
            else
                // The operation failed.
                return BadRequest();

            // Obtain an array of type 'Request'.
            Request[] requests = _dbContext.Request.Where(i => i.HoursWorked >= 6 && i.HourlyRate >= 300).ToArray();

            // Iterate through the collection.
            for (int i = 0; i < requests.Length; i++)
            {
                requests[i].RequestStatus = "Approved";
                _dbContext.Request.Update(requests[i]);
            }

            // Save the changes asynchronously.
            await _dbContext.SaveChangesAsync();

            // Iterate through the collection.
            for (int i = 0; i < requests.Length; i++)
            {
                // Declare and instantiate a RequestProcess object.
                RequestProcess process = new RequestProcess();
                process.RequestID = requests[i].RequestID;
                process.ManagerID = managerID;
                process.Status = "Approved";
                process.Date = DateTime.Now;

                // Add the RequestProcess object to the RequestProcess table.
                _dbContext.RequestProcess.Add(process);
            }

            // Save the changes asynchronously.
            await _dbContext.SaveChangesAsync();

            // The operation succeeded.
            return Ok();
        }

        // ------------------ HTTP PUT ------------------

        /// <summary>
        /// Updates a lecturer's account.
        /// </summary>
        /// <param name="lecturerId"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="identityNumber"></param>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        [Route("api/Database/UpdateLecturerAccount")]
        [HttpPut]
        public async Task<ActionResult> UpdateLecturerAccount(int lecturerId, string firstName, string lastName, string identityNumber, string emailAddress)
        {
            try
            {
                // Obtain the lecturer object.
                Lecturer lecturer = _dbContext.Lecturer.Where(i => i.LecturerID == lecturerId).ToArray()[0];

                // Update the properties.
                lecturer.FirstName = firstName;
                lecturer.LastName = lastName;
                lecturer.IdentityNumber = identityNumber;
                lecturer.EmailAddress = emailAddress;

                // Update the Lecturer table.
                _dbContext.Lecturer.Update(lecturer);
                // Save the changes asynchronously.
                await _dbContext.SaveChangesAsync();

                // The operation succeeded.
                return Ok();
            }
            catch
            {
                // The operation failed.
                return BadRequest();
            }
        }

        /// <summary>
        /// Updates a manager's account.
        /// </summary>
        /// <param name="managerId"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="identityNumber"></param>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        [Route("api/Database/UpdateManagerAccount")]
        [HttpPut]
        public async Task<ActionResult> UpdateManagerAccount(int managerId, string firstName, string lastName, string identityNumber, string emailAddress)
        {
            try
            {
                // Obtain the manager object.
                Manager manager = _dbContext.Manager.Where(i => i.ManagerID == managerId).ToArray()[0];

                // Update the properties.
                manager.FirstName = firstName;
                manager.LastName = lastName;
                manager.IdentityNumber = identityNumber;
                manager.EmailAddress = emailAddress;

                // Update the Manager table.
                _dbContext.Manager.Update(manager);
                // Save the changes asynchronously.
                await _dbContext.SaveChangesAsync();

                // The operation succeeded.
                return Ok();
            }
            catch
            {
                // The operation failed.
                return BadRequest();
            }
        }

        /// <summary>
        /// Updates the claim.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="requestFor"></param>
        /// <param name="hoursWorked"></param>
        /// <param name="hourlyRate"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        [Route("api/Database/UpdateRequest")]
        [HttpPut]
        public async Task<ActionResult> UpdateRequest(int requestId, string requestFor, double hoursWorked, double hourlyRate, string description)
        {
            try
            {
                // Declare a Request object.
                Request request = _dbContext.Request.Where(i => i.RequestID == requestId).ToArray()[0];

                // Update the properties.
                request.RequestFor = requestFor;
                request.HoursWorked = hoursWorked;
                request.HourlyRate = hourlyRate;
                request.Description = description;

                // Update the Request table.
                _dbContext.Request.Update(request);
                // Save the changes asynchronously.
                await _dbContext.SaveChangesAsync();

                // The operation succeeded.
                return Ok();
            }
            catch
            {
                // The operation failed.
                return BadRequest();
            }
        }

        // ------------------ HTTP DELETE ------------------

        /// <summary>
        /// Deletes a lecturer's account.
        /// </summary>
        /// <param name="lecturerId"></param>
        /// <returns></returns>
        [Route("api/Database/DeleteLecturerAccount")]
        [HttpDelete]
        public async Task<ActionResult> DeleteLecturerAccount(int lecturerId)
        {
            try
            {
                // Get the lecturer object.
                Lecturer lecturer = _dbContext.Lecturer.Where(i => i.LecturerID == lecturerId).ToArray()[0];

                // Remove data from the Lecturer table.
                _dbContext.Lecturer.Remove(lecturer);
                // Save the changes asynchronously.
                await _dbContext.SaveChangesAsync();

                // The operation succeeded.
                return Ok();
            }
            catch
            {
                // The operation failed.
                return BadRequest();
            }
        }

        /// <summary>
        /// Deletes a claim.
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        [Route("api/Database/DeleteRequest")]
        [HttpDelete]
        public async Task<ActionResult> DeleteRequest(int requestId)
        {
            try
            {
                // Create a Request object.
                Request request = _dbContext.Request.Where(i => i.RequestID == requestId).ToArray()[0];

                // Remove data from the Request table.
                _dbContext.Request.Remove(request);
                // Save the changes asynchronously.
                await _dbContext.SaveChangesAsync();

                // The operation succeeded.
                return Ok();
            }
            catch
            {
                // The operation failed.
                return BadRequest();
            }
        }
    }
}
