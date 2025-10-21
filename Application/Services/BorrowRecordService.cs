﻿using Application.DTOs;
using Application.IRepository;
using Application.IService;
using Application.Mappers;
using Domain.Entities;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class BorrowRecordService : IBorrowRecordService
    {
        //TODO import more than one interface and check for member and book in borrow record this breaks single responsibility?
        private readonly IBorrowRecordRepository _repository;
        private readonly IBookRepository _bookrepository;
        private readonly IMemberRepository _memberrepository;

        public BorrowRecordService(IBorrowRecordRepository repository, IBookRepository bookrepository, IMemberRepository memberrepository)
        {
            _repository = repository;
            _bookrepository = bookrepository;
            _memberrepository = memberrepository;
        }

        public async Task<BorrowRecordResponseDto> BorrowBook(int bookID,string userEmail)
        {
            bool bookExists = await _bookrepository.CheckExistsAsync(bookID);
            if (!bookExists) throw new BookDoesnotExist($"No Book Exists with this ID {bookID}");
            bool bookAvailable = await _bookrepository.CheckAvailableAsync(bookID);
            if (!bookAvailable) return null;
            Book book = await _bookrepository.GetBookAsync(bookID);
            book.IsAvailable = false;
            await _bookrepository.UpdateBookAsync(bookID, book);
            Member user = await _memberrepository.GetMemberAsyncByEmail(userEmail);
            BorrowRecord borrowRecord =  await _repository.BorrowBookAsync(new BorrowRecord
            {
                BookId = bookID,
                MemberId = user.Id,
                Member = user,
                Book = book,
                BorrowDate = DateOnly.FromDateTime(DateTime.UtcNow)
            });
            return borrowRecord.BorrowRecordtoDto();
        }
        public async Task<BorrowRecordResponseDto> ReturnBook(int bookID,string userEmail)
        {
            Member user = await _memberrepository.GetMemberAsyncByEmail(userEmail);
            bool recordExists = await _repository.CheckExistsAsync(bookID, user.Id);
            if (!recordExists) throw new RecordDoesnotExist($"this user {userEmail} did not borrow this book {bookID}");
            BorrowRecord borrowRecord = await _repository.GetBorrowRecordAsync(bookID, user.Id);
            if (borrowRecord.ReturnDate != null) return null;
            Book book = await _bookrepository.GetBookAsync(bookID);
            book.IsAvailable = true;
            await _bookrepository.UpdateBookAsync(bookID, book);
            borrowRecord =  await _repository.ReturnBookAsync(borrowRecord.Id,DateOnly.FromDateTime(DateTime.UtcNow));
            return borrowRecord.BorrowRecordtoDto();

        }
    }
}
