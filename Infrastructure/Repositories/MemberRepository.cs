using Application.IRepository;
using Domain.Entities;
using FluentEmail.Core;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class MemberRepository:IMemberRepository
    {
        private readonly LibraryDbContext _context;

        public MemberRepository(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<Member> AddMemberAsync(Member member)
        {
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task<bool> CheckExistsAsyncById(string Id)
        {
            return await _context.Members.AnyAsync(m=>m.Id == Id);

        }
        public async Task<bool> CheckExistsAsyncByEmail(string Email)
        {
            return await _context.Members.AnyAsync(m => m.Email == Email);
        }
        public async Task<Member?> GetMemberAsyncById(string Id)
        {
            return await _context.Members.FirstOrDefaultAsync(m => m.Id == Id);
        }
        public async Task<Member?> GetMemberAsyncByEmail(string Email)
        {
            return await _context.Members.FirstOrDefaultAsync(m => m.Email == Email);
        }
        public async Task<ICollection<Member>> GetMembersAsync()
        {
            return await _context.Members.ToListAsync();
        }

        public async Task<bool> editMemberAsync(string Email, Member newMember)
        {
            var member = await _context.Members
           .FirstOrDefaultAsync(m => m.Email == Email);

            if (member == null)
            {
                return false; // member not found
            }

            member = newMember;
            _context.Members.Update(member);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
