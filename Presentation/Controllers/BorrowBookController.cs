using Application.IService;
using Application.Results;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Controller]
    public class BorrowBookController : ControllerBase
    {
        private readonly IBorrowRecordService borrowRecordService;

        public BorrowBookController(IBorrowRecordService borrowRecordService)
        {
            this.borrowRecordService = borrowRecordService;
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        [Route("/api/borrow/{BookId}")]
        public async Task<IActionResult> BorrowBook(int BookId)
        {
            //try
            //{
                string Email = User.FindFirst(ClaimTypes.Email)?.Value;
                var borrowRecordResult = await borrowRecordService.BorrowBook(BookId, Email);
            if (!borrowRecordResult.IsSuccess) {
                if (borrowRecordResult.error == Errors.DoesntExist) return NotFound(borrowRecordResult);
                if (borrowRecordResult.error == Errors.notAvailable) return BadRequest(borrowRecordResult);
            }
            return Ok(borrowRecordResult);
            //if(borrowRecord == null)
                //{
                //    return BadRequest("Book Is not available to borrow");
                //}
                //return Ok(borrowRecord);
            //}
            //catch (BookDoesnotExist ex)
            //{
            //    return BadRequest(ex.Message);
            //}
            //catch (Exception e)
            //{ return BadRequest(e.Message); }
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        [Route("/api/return/{BookId}")]
        public async Task<IActionResult> ReturnBook(int BookId)
        {
            try {
                string Email = User.FindFirst(ClaimTypes.Email)?.Value;
                var borrowRecord = await borrowRecordService.ReturnBook(BookId, Email);
                if (borrowRecord == null) return BadRequest("This book was returned");
                return Ok(borrowRecord);
            }catch(RecordDoesnotExist ex)
            {
                return BadRequest(ex.Message);
            }catch(Exception e)
            {
                return BadRequest(e.Message);
            }
            }

    }
}