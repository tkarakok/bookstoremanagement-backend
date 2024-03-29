﻿using Business.Abstract;
using Business.Constants;
using Core.Entities.Concrete;
using Core.Entities.DTOs;
using Core.Utilities.Results;
using Core.Utilities.Security.Hashing;
using Core.Utilities.Security.JWT;
namespace Business.Concrete
{
    public class AuthManager : IAuthService
    {
        private IEmployeeService _userService;
        private ITokenHelper _tokenHelper;

        public AuthManager(IEmployeeService userService, ITokenHelper tokenHelper)
        {
            _userService = userService;
            _tokenHelper = tokenHelper;
        }

        public IDataResult<Employee> Register(EmployeeForRegisterDto userForRegisterDto, string password)
        {
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(password, out passwordHash, out passwordSalt);
            var user = new Employee
            {
                Email = userForRegisterDto.Email,
                FirstName = userForRegisterDto.FirstName,
                LastName = userForRegisterDto.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Status = true,
                ClaimId = 1
            };
            _userService.Add(user);

            //var employeeId = _userService.GetByMail(userForRegisterDto.Email).Id;
            //var claim = new EmployeeOperationClaim
            //{
            //    EmployeeId = employeeId,
            //    OperationClaimId = 1
            //};
            //_operationClaimService.Add(claim);
            return new SuccessDataResult<Employee>(user, Messages.UserRegistered);
        }

        public IDataResult<Employee> Login(EmployeeForLoginDto userForLoginDto)
        {
            var userToCheck = _userService.GetByMail(userForLoginDto.Email);
            if (userToCheck == null)
            {
                return new ErrorDataResult<Employee>(Messages.UserNotFound);
            }

            if (!HashingHelper.VerifyPasswordHash(userForLoginDto.Password, userToCheck.PasswordHash, userToCheck.PasswordSalt))
            {
                return new ErrorDataResult<Employee>(Messages.PasswordError);
            }

            return new SuccessDataResult<Employee>(userToCheck, Messages.SuccessfulLogin);
        }

        public IResult UserExists(string email)
        {
            if (_userService.GetByMail(email) != null)
            {
                return new ErrorResult(Messages.UserAlreadyExists);
            }
            return new SuccessResult();
        }

        public IDataResult<AccessToken> CreateAccessToken(Employee user)
        {
            var claims = _userService.GetClaims(user);
            if (claims.Count == 0)
            {
                //throw new Exception();
            }
            var accessToken = _tokenHelper.CreateToken(user, claims);
            return new SuccessDataResult<AccessToken>(accessToken, Messages.AccessTokenCreated);
        }
    }
}



