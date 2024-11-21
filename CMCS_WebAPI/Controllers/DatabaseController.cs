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

        [Route("api/Database/AddRecoveryMethod")]
        [HttpPost]
        public async Task<ActionResult> AddRecoveryMethod(string type, string identityNumber, string value)
        {
            if (type == "FILE")
            {
                AccountRecovery recovery = new AccountRecovery();
                recovery.Method = "FILE";
                recovery.UserID = identityNumber;
                recovery.Value = value;

                _dbContext.AccountRecovery.Add(recovery);
                await _dbContext.SaveChangesAsync();

                return Ok();
            }

            if (type == "QUESTION")
            {
                AccountRecovery recovery = new AccountRecovery();
                recovery.Method = "QUESTION";
                recovery.UserID = identityNumber;
                recovery.Value = value;

                _dbContext.AccountRecovery.Add(recovery);
                await _dbContext.SaveChangesAsync();

                return Ok();
            }

            return BadRequest();
        }

        [Route("api/Database/RemoveRecoveryMethod")]
        [HttpPost]
        public async Task<ActionResult> RemoveRecoveryMethod(string type, string identityNumber)
        {
            int index = -1;
            AccountRecovery[] list = _dbContext.AccountRecovery.ToArray();

            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].UserID == identityNumber && list[i].Method == type)
                {
                    index = i;
                    break;
                }
            }

            if(index != -1)
            {
                _dbContext.AccountRecovery.Remove(list[index]);
                await _dbContext.SaveChangesAsync();

                return Ok();
            }

            return BadRequest();
        }

        [Route("api/Database/ApproveRequest")]
        [HttpPost]
        public async Task<ActionResult> ApproveRequest(Request request)
        {
            int managerID = 0;
            Manager[] managers = _dbContext.Manager.ToArray();

            if (managers.Length > 0)
            {
                managerID = managers[0].ManagerID;
            }
            else
                return BadRequest();


            request.RequestStatus = "Approved";
            _dbContext.Request.Update(request);
            await _dbContext.SaveChangesAsync();

            RequestProcess process = new RequestProcess();
            process.RequestID = request.RequestID;
            process.ManagerID = managerID;
            process.Status = "Approved";
            process.Date = DateTime.Now;

            _dbContext.RequestProcess.Add(process);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [Route("api/Database/RejectRequest")]
        [HttpPost]
        public async Task<ActionResult> RejectRequest(Request request)
        {
            int managerID = 0;
            Manager[] managers = _dbContext.Manager.ToArray();

            if (managers.Length > 0)
            {
                managerID = managers[0].ManagerID;
            }
            else
                return BadRequest();


            request.RequestStatus = "Rejected";
            _dbContext.Request.Update(request);
            await _dbContext.SaveChangesAsync();

            RequestProcess process = new RequestProcess();
            process.RequestID = request.RequestID;
            process.ManagerID = managerID;
            process.Status = "Rejected";
            process.Date = DateTime.Now;

            _dbContext.RequestProcess.Add(process);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [Route("api/Database/ProcessAllRequests")]
        [HttpPost]
        public async Task<ActionResult> ProcessAllRequests()
        {
            int managerID = 0;
            Manager[] managers = _dbContext.Manager.ToArray();

            if (managers.Length > 0)
            {
                managerID = managers[0].ManagerID;
            }
            else
                return BadRequest();

            Request[] requests = _dbContext.Request.ToArray();

            for (int i = 0; i < requests.Length; i++)
            {
                if (requests[i].HoursWorked >= 6 && requests[i].HourlyRate >= 300)
                {
                    requests[i].RequestStatus = "Approved";
                    _dbContext.Request.Update(requests[i]);
                }
            }

            await _dbContext.SaveChangesAsync();

            for (int i = 0; i < requests.Length; i++)
            {
                if (requests[i].HoursWorked >= 6 && requests[i].HourlyRate >= 300)
                {
                    RequestProcess process = new RequestProcess();
                    process.RequestID = requests[i].RequestID;
                    process.ManagerID = managerID;
                    process.Status = "Approved";
                    process.Date = DateTime.Now;

                    _dbContext.RequestProcess.Add(process);
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        // ------------------ HTTP PUT ------------------

        [Route("api/Database/UpdateLecturerAccount")]
        [HttpPut]
        public async Task<ActionResult> UpdateLecturerAccount(Lecturer lecturer)
        {
            _dbContext.Lecturer.Update(lecturer);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [Route("api/Database/UpdateManagerAccount")]
        [HttpPut]
        public async Task<ActionResult> UpdateManagerAccount(Manager manager)
        {
            _dbContext.Manager.Update(manager);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        public async Task<ActionResult> UpdateRequest(Request request)
        {
            _dbContext.Request.Update(request);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        // ------------------ HTTP DELETE ------------------

        [Route("api/Database/DeleteLecturerAccount")]
        [HttpDelete]
        public async Task<ActionResult> DeleteLecturerAccount(Lecturer lecturer)
        {
            _dbContext.Lecturer.Remove(lecturer);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [Route("api/Database/DeleteRequest")]
        [HttpDelete]
        public async Task<ActionResult> DeleteRequest(Request request)
        {
            _dbContext.Request.Remove(request);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
