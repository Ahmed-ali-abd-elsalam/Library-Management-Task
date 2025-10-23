using Application.DTOs;
using Application.Results;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IBorrowRecordService
    {
        public Task<Result<BorrowRecordResponseDto>> BorrowBook(int bookID, string userEmail);
        public Task<BorrowRecordResponseDto?> ReturnBook(int bookID, string userEmail);


    }
}
