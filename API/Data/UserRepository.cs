using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _cotnext;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _cotnext = context;

        }

        public async Task<MemberDto> GetMemberAsync(string username){
            return await _cotnext.Users
                    .Where(x => x.UserName == username)
                    .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                    .SingleOrDefaultAsync();



        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams){
            var query =  _cotnext.Users.AsQueryable();
            query = query.Where(u => u.UserName != userParams.CurrentUsername);
            query = query.Where(u => u.Gender == userParams.Gender);

            var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
            var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAGe));

            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            query = userParams.OrderBy switch {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };

            return await PagedList<MemberDto>
                .CreateAsync(query.AsNoTracking()
                    .ProjectTo<MemberDto>(
                    _mapper.ConfigurationProvider),
                 userParams.PageNumber,
                 userParams.PageSize
                );
            
        }

        public async Task<AppUser> GetUserByIdAsync(int id){   
            return await _cotnext.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username){
            return await _cotnext.Users
                    .Include(p => p.Photos)
                    .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<string> GetUserGender(string username)
        {

            return await _cotnext.Users.Where(x => x.UserName == username)
                    .Select( x=> x.Gender)
                    .FirstOrDefaultAsync();

        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync(){
            return await _cotnext.Users
                    .Include(p => p.Photos)
                    .ToListAsync();
        }

        

        public void Update(AppUser user){

            _cotnext.Entry(user).State = EntityState.Modified;
        }
    }
}