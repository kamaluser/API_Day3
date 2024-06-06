using Course.Core.Entities;
using Course.Data;
using Course.Service.Dtos.StudentDtos;
using Course.Service.Exceptions;
using Course.Service.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Course.Service.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _context;

        public StudentService(AppDbContext context)
        {
            _context = context;
        }
        public int Create(StudentCreateDto dto)
        {
            if (_context.Students.Any(x => x.Email == dto.Email))
                throw new DuplicateEntityException($"Student with email {dto.Email} already exists.");

            Group group = _context.Groups.Include(x => x.Students).FirstOrDefault(x => x.Id == dto.GroupId && !x.IsDeleted);

            if (group == null)
                throw new EntityNotFoundException($"Group with ID {dto.GroupId} not found.");

            if (group.Limit <= group.Students.Count)
                throw new GroupLimitException($"Group is full!");

            Student student = new Student
            {
                FullName = dto.FullName,
                Email = dto.Email,
                BirthDate = dto.BirthDate,
                GroupId = dto.GroupId
            };

            _context.Students.Add(student);
            _context.SaveChanges();
            return student.Id;
        }

        public List<StudentGetDto> GetAll()
        {
            var students = _context.Students.Include(x => x.Group).Where(x=>!x.IsDeleted).Select(x => new StudentGetDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                BirthDate = x.BirthDate,
                GroupId = x.GroupId,
                GroupName = x.Group.No
            }).ToList();
            return students;
        }

        public StudentGetDto GetById(int id)
        {
            var student = _context.Students.Include(x => x.Group).FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (student == null)
                throw new EntityNotFoundException($"Student with {id} ID not found.");

            return new StudentGetDto
            {
                Id = student.Id,
                FullName = student.FullName,
                Email = student.Email,
                BirthDate = student.BirthDate,
                GroupId = student.GroupId,
                GroupName = student.Group.No
            };
        }
        public void Edit(int id, StudentEditDto dto)
        {
            var student = _context.Students.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (student == null)
                throw new EntityNotFoundException($"Student with {id} ID not found.");

            Group group = _context.Groups.Include(x => x.Students).FirstOrDefault(x => x.Id == dto.GroupId && !x.IsDeleted);

            if (group == null)
                throw new EntityNotFoundException($"Group with ID {dto.GroupId} not found.");

            if (group.Limit <= group.Students.Count)
                throw new GroupLimitException($"Group is full!");

            student.FullName = dto.FullName;
            student.Email = dto.Email;
            student.BirthDate = dto.BirthDate;
            student.GroupId = dto.GroupId;

            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var student = _context.Students.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
            if (student == null)
                throw new EntityNotFoundException($"Student with {id} ID not found.");

            student.IsDeleted = true;
            _context.Students.Remove(student);
            _context.SaveChanges();
        }
    }
}
