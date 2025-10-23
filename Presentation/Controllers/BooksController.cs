using Application.DTOs;
using Application.IService;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/books/")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        [ProducesResponseType(statusCode: 200, type: typeof(BooksPaginatedDto))]
        public async Task<IActionResult> getBooks([FromQuery] BooksFilter booksFilter,int offest = 0, int count = 100)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(await _bookService.GetAllBooks(offest, count,booksFilter));
        }

        [HttpPost("/api/books/add")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(statusCode: 200,type: typeof(BookResponseDto))]
        public async Task<IActionResult> AddBook([FromBody]BookDto bookDto)

        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(await _bookService.AddNewBook(bookDto));
        }
        
        [HttpPut("{BookId}")]
        [Authorize(Roles ="Admin")]
        [ProducesResponseType(statusCode: 200, type: typeof(BookResponseDto))]
        public async Task<IActionResult> UpdateBook(int BookId,BookDto bookDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var book =await _bookService.UpdateBook(bookDto,BookId);
            if (book == null) return NotFound();
            return Ok(book);
        }
        
        [HttpDelete("{BookId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(statusCode: 200, type: typeof(BookResponseDto))]
        public async Task<IActionResult> Delete(int BookId)
        {
            try
            {
                var book = await _bookService.DeleteBook(BookId);
                return Accepted();
            }
            catch (BookDoesnotExist e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NoContent();
            }
        }



    }
}
