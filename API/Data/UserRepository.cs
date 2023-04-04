using API.DTOs;
using API.Entities;
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

        public async Task<IEnumerable<MemberDto>> GetMembersAsync(){
            return await _cotnext.Users
                    .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

        }

        public async Task<AppUser> GetUserByIdAsync(int id){   
            return await _cotnext.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username){
            return await _cotnext.Users
                    .Include(p => p.Photos)
                    .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync(){
            return await _cotnext.Users
                    .Include(p => p.Photos)
                    .ToListAsync();
        }

        public async Task<bool> SaveAllAsync(){
            return await _cotnext.SaveChangesAsync() > 0;

        }

        public void Update(AppUser user){

            _cotnext.Entry(user).State = EntityState.Modified;
        }
    }
}