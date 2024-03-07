using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using Api.Models;
using AutoMapper;
using DataAccessLayer.Entities;
using DataAccessLayer.Context;
using DataAccessLayer.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Smartmenu.Infrastructure;
using System.Text.Json;
using Api.Services;
using DataAccessLayer.Repository;
using System.Threading.Tasks;
using DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.Cors;
using WebPush;

namespace Api.Controllers
{
    [ApiController]
    //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

    public class SecurityController : ControllerBase
    {
        public IConfiguration _config;
        private const string AUTH_HEADER_NAME = "Authorization";
        private const string AUTH_METHOD_NAME = "Basic ";

        private readonly SmartmenuDbContext _context;
        private readonly IMapper _mapper;
        private ICurrentUserService _currentUserService;
        private IRepository<User> _repository;
        private IRepository<GuestRoomMapping> _guestroomrepository;
        private IRepository<HotelGuest> _guestRepository;
        private IRepository<HotelRoom> _roomRepository;
        private IRepository<Hotel> _hotelRepository;
        public SecurityController(
            IMapper mapper,
            IConfiguration configuration,
            ICurrentUserService currentUserService,
            SmartmenuDbContext context)
        {
            _mapper = mapper;
            _config = configuration;
            _context = context;
            _currentUserService = currentUserService;
        }

        [Route("api/login")]
        [HttpPost]
        public IActionResult GetToken()
        {
            User appUser = new User();
            bool result = ValidateHeader(Request, ref appUser);
            if (result)
            {
                var tokenString = GenerateJWT(appUser);
                UserVM appUserVM = new UserVM();
                appUserVM = _mapper.Map<UserVM>(appUser);
                UserClientMapping userClientMapping = _context.UserClientMapping.FirstOrDefault(a => a.UserId == appUser.Id);
                Client client = userClientMapping != null ? _context.Client.Include(c => c.Labels).FirstOrDefault(client => client.Id == userClientMapping.ClientId) : null;
                ClientVM clientVM = _mapper.Map<ClientVM>(client);
                if (userClientMapping != null)
                {
                    var subscriptionnData = _context.Subscription.FirstOrDefault(x => x.ClientId == userClientMapping.ClientId);
                    if (subscriptionnData != null)
                    {
                        //HotelVM1.MaxCount = subscriptionnData.MaxSMS - subscriptionnData.SMSCount;
                        clientVM.MaxSMS = subscriptionnData.MaxSMS;
                        clientVM.MaxCount= subscriptionnData.SMSCount;
                    }
                }
                Hotel hotel = null;
                if(appUser.UserType != 5)
                {
                    UserHotelMapping userHotelMapping = _context.UserHotelMapping.FirstOrDefault(a => a.UserId == appUserVM.Id);
                    hotel = userHotelMapping != null ? _context.Hotel.Include(c => c.Labels).FirstOrDefault(h => h.Id == userHotelMapping.HotelId) : null;
                }
                else
                {
                    UserServicesMapping userServiceMapping = _context.UserServicesMapping.Include(a => a.Services).FirstOrDefault(a => a.UserId == appUserVM.Id);
                    hotel = userServiceMapping != null ? _context.Hotel.FirstOrDefault(h => h.Id == userServiceMapping.Services.ClientId) : null;
                }

                HotelVM hotelVM = _mapper.Map<HotelVM>(hotel);
                PermissionsGroup permissionsGroup = _context.PermissionsGroup.FirstOrDefault(b => b.Id == appUserVM.PermissionGroupId);
                appUserVM.PermissionsGroup = _mapper.Map<PermissionsGroupVM>(permissionsGroup);
                appUserVM.Password = "";
                return Ok(new { token = tokenString, user = appUserVM,client = clientVM, hotel = hotelVM });
            }
            else
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [Route("api/isAuthenticated")]
        [HttpPost]
        public IActionResult IsAuthenticated()
        {
            AuthenticatedVM authenticatedVM = new AuthenticatedVM();
            authenticatedVM.Authenticated = true;
            authenticatedVM.User = _mapper.Map<UserVM>(_currentUserService.User);
            return Ok(authenticatedVM);
        }

        private string GenerateJWT(User user)
        {
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var expiry = DateTime.Now.AddYears(1);
            var securityKey = new SymmetricSecurityKey
        (Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials
        (securityKey, SecurityAlgorithms.HmacSha256);
            user.Password = "";
            user.PasswordSalt = "";
            List<Claim> claims;
            if (user.UserType == 2 || user.UserType == 3)
            {
                UserClientMapping userClientMapping = _context.UserClientMapping.FirstOrDefault(a => a.UserId == user.Id);
                Client client = userClientMapping != null ? _context.Client.FirstOrDefault(client => client.Id == userClientMapping.ClientId) : null;
                claims = new List<Claim>() {
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                        new Claim("AppUserData", Convert.ToBase64String(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(user))),
                        new Claim("ClientData", JsonConvert.SerializeObject(client)),
                        new Claim("HotelData", JsonConvert.SerializeObject(null)),
                        new Claim("UserName",user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Role, user.Role)
                    };
            }
            else if(user.UserType == 4)
            {
                UserHotelMapping userHotelMapping = _context.UserHotelMapping.FirstOrDefault(a => a.UserId == user.Id);
                Hotel hotel = userHotelMapping != null ? _context.Hotel.FirstOrDefault(h => h.Id == userHotelMapping.HotelId) : null;
                claims = new List<Claim>() {
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                        new Claim("AppUserData", Convert.ToBase64String(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(user))),
                        new Claim("HotelData", JsonConvert.SerializeObject(hotel)),
                        new Claim("ClientData", JsonConvert.SerializeObject(null)),
                        new Claim("UserName",user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Role, user.Role)
                    };
            }
            else
            {
                UserServicesMapping userServiceMapping = _context.UserServicesMapping.Include(a => a.Services).FirstOrDefault(a => a.UserId == user.Id);
                Hotel hotel = userServiceMapping != null ? _context.Hotel.FirstOrDefault(h => h.Id == userServiceMapping.Services.ClientId) : null;
                claims = new List<Claim>() {
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                        new Claim("AppUserData", Convert.ToBase64String(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(user))),
                        new Claim("UserName",user.UserName),
                        new Claim("HotelData", JsonConvert.SerializeObject(hotel)),
                        new Claim("ClientData", JsonConvert.SerializeObject(null)),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Role, user.Role)
                    };
            }
           

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiry,
                signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            var stringToken = tokenHandler.WriteToken(token);
            return stringToken;
        }

        private bool ValidateHeader(HttpRequest request, ref User user)
        {
            string authHeader = request.Headers.Keys.Contains(AUTH_HEADER_NAME) ? request.Headers[AUTH_HEADER_NAME].First() : string.Empty;
            string orderToken = ((authHeader != null && authHeader.StartsWith(AUTH_METHOD_NAME)) ? authHeader.Substring(AUTH_METHOD_NAME.Length) : string.Empty);
            if (string.IsNullOrEmpty(orderToken) || orderToken.Equals("null", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new AuthenticationException("You must send your credentials using Authorization header");
            }
            else
            {
                var base64EncodedBytes = Convert.FromBase64String(orderToken);
                 
                string plainHeaderData = Encoding.UTF8.GetString(base64EncodedBytes);
                string userName = plainHeaderData?.Split(':')[0];
                string password = plainHeaderData?.Split(':')[1];
                User appUser = user;
                if (CheckLogin(userName, password, ref appUser))
                {
                    user = appUser;
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        private bool CheckLogin(string userName, string password, ref User user)
        {
            bool result = false;
            try
            {
                user = _context.User.FirstOrDefault(user => user.UserName == userName);

                //check user
                if (user != null && user.Status)
                {
                    byte[] passwordHashBytes = Convert.FromBase64String(user.Password);
                    byte[] passwordSaltBytes= Convert.FromBase64String(user.PasswordSalt);
                   // passwordSaltBytes = Convert.FromBase64String(user.PasswordSalt);
                    result = PasswordHasher.VerifyPassword(password, passwordSaltBytes, passwordHashBytes);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }


        [Route("api/forget")]
        [HttpPost]
        public async Task<IActionResult> Forget([FromBody] UserVM user)
        {
      

            User appUser = _context.User.FirstOrDefault(u => u.Email == user.Email );
            if (appUser != null)
            {
                appUser = await  SendUserOTPAsync(appUser);
                UserVM appUserVM = new UserVM();
                appUserVM = _mapper.Map<UserVM>(appUser);
                return Ok(true);
            }
            else
            {
                return Ok(null);
            }
        }


        private async Task<User> SendUserOTPAsync(User appUser)
        {
            _repository = new UserRepository(_context);
            string customerEmail = Convert.ToString(appUser.Email);
            if (!string.IsNullOrEmpty(customerEmail))
            {
                StringBuilder emailMessage = new StringBuilder();
                string otp = Convert.ToBase64String(PasswordHasher.GenerateSalt()).Substring(1, 6);
                byte[] userSalt = Convert.FromBase64String(appUser.PasswordSalt);
                appUser.OTP = Convert.ToBase64String(PasswordHasher.ComputeHash(otp, userSalt));
                appUser.OTPExpiry = DateTime.Now.AddHours(1).ToString();
                emailMessage.Append("<!DOCTYPE html> " +
"<html>" +
"<body>" +
    "<table width='100%' border='0' align='center' cellpadding='0' cellspacing='0'>" +


        "<tbody>" +
            "<tr>" +
                "<td align='center'>" +
                    "<table class='col-600' width='600' style='border:1px solid black' align='center' cellpadding='0' cellspacing='0'>" +
                        "<tbody>" +
                            "<tr>" +
                                "<td align='center' valign='top' bgcolor='#fff' style='background-size:cover; background-position:top;height:400;'>" +
                                    "<table class='col-600' width='600' height='400' border='0' align='center' cellpadding='0' cellspacing='0'>" +

                                        "<tbody>" +
                                           "<tr>" +
                                                "<td height='40'>" + "</td>" +
                                            "</tr>" +


                                            "<tr>" +
                                                "<td align='center' style='line-height: 0px;'>" +
                                                "</td>" +
                                            "</tr>" +


                                            "<tr>" +
                                                "<td align='center' style='font-family: Raleway, sans-serif; font-size:37px; color:#002535; line-height:50px; font-weight: bold; letter-spacing: 7px;'>" +
                                                    "OTP <span style='font-family: Raleway, sans-serif; font-size:37px; color:#c61c1d; line-height:39px; font-weight: 300; letter-spacing: 7px;'>Verification</span>" +
                                                "</td>" +
                                            "</tr>" +





                                            "<tr>" +
                                                "<td align='center' style=\"font-family: 'Lato', sans-serif; font-size:15px; color:#002535; line-height:24px; font-weight: 300;\">" +
                                                    "Dear Customer," +
                                                    "<br/>" +
                                                    "Your One Time Password is <br/>" +
                                                    "<p style='    display: inline-block;" +
    "margin-bottom: 0;" +
    "font-weight: normal;" +
    "text-align: center;" +
    "vertical-align: middle;" +
    "cursor: pointer;" +
    "background-image: none;" +
    "border: 1px solid transparent;" +
    "white-space: nowrap;" +
    "padding: 6px 12px;" +
    "text-decoration: none;" +
    "font-size: 14px;" +
    "line-height: 1.42857143;" +
    "border-radius: 4px;" +
    "-webkit-user-select: none;" +
    "-moz-user-select: none;" +
    "-ms-user-select: none;" +
    "user-select: none;background: #f5cb39;" +
    "color: #fff !important;" +
    "box-shadow: 0px 4px #be4006;" +
    "border: 1px solid #f5cb39;'>" + otp + "</p>" +
                                                    "<br/>" + "<br/>" +
                                                    "عزيري العميل" +
"<br/>" +
"كلمة المرور المؤقتة الخاصة بحسابك هي" +
"<h2>" + "<strong>" + "</strong>" + "</h2>" +
"<br/>" +
"<p style='    display: inline-block;" +
    "margin-bottom: 0;" +
    "font-weight: normal;" +
    "text-align: center;" +
    "vertical-align: middle;" +
    "cursor: pointer;" +
    "background-image: none;" +
    "border: 1px solid transparent;" +
    "white-space: nowrap;" +
    "text-decoration: none;" +
    "padding: 6px 12px;" +
    "font-size: 14px;" +
    "line-height: 1.42857143;" +
    "border-radius: 4px;" +
    "-webkit-user-select: none;" +
    "-moz-user-select: none;" +
    "-ms-user-select: none;" +
    "user-select: none;background: #f5cb39;" +
    "color: #fff !important;" +
    "box-shadow: 0px 4px #be4006;" +
    "border: 1px solid #f5cb39;'> " + otp + " </p>" +
                                                "</td>" +
                                            "</tr>" +


                                            "<tr>" +
                                                "<td height='50'>" + "</td>" +
                                            "</tr>" +
                                        "</tbody>" +
                                    "</table>" +
                                "</td>" +
                            "</tr>" +
                        "</tbody>" +
                    "</table>" +
                "</td>" +
            "</tr>" +







        "</tbody>" +
    "</table>" +
"</body>" +
"</html>");
                string emailMsg = emailMessage.ToString();

                await EmailService.SendAsync(_context,"info@GetSmartMenu.com", new string[] { customerEmail }, "One Time Password", emailMsg);
                await _repository.UpdateAsync(appUser);
                await _repository.SaveChangesAsync();
            }
            
            return appUser;
        }


        [Route("api/otplogin")]
        [HttpPost]
        public async Task<IActionResult> OTPLogin([FromBody] UserVM user)
        {
            bool result = false;
            if (user != null)
            {
                User appUser = _context.User.FirstOrDefault(u => u.Email == user.Email);

                if (appUser != null)
                {
                    if (Convert.ToDateTime(appUser.OTPExpiry) >= System.DateTime.Now)
                    {
                        byte[] passwordHashBytes = Convert.FromBase64String(appUser.OTP);
                        byte[] passwordSaltBytes;
                        passwordSaltBytes = Convert.FromBase64String(appUser.PasswordSalt);

                        result = PasswordHasher.VerifyPassword(user.OTP, passwordSaltBytes, passwordHashBytes);
                        if (result)
                        {
                            appUser.OTP = null;
                            appUser.OTPExpiry = null;
                            //appUser.Status = true;
                            var password = user.Password;
                            byte[] userSalt = PasswordHasher.GenerateSalt();
                            appUser.Password = Convert.ToBase64String(PasswordHasher.ComputeHash(Convert.ToString(password), userSalt));
                            appUser.LastLogin = DateTime.Now;
                            appUser.PasswordSalt = Convert.ToBase64String(userSalt);
                            _repository = new UserRepository(_context);
                            await _repository.UpdateAsync(appUser);
                            await _repository.SaveChangesAsync();
                        }
                        
                    }
                }
            }

            return Ok(result);
        }


        [Route("api/guestlogin")]
        [HttpPost]
        public async Task<IActionResult> GuestLogin([FromBody] GuestRoomMappingVM guestRoomMappingVM)
        {
            User appUser = new User();
            bool result = Convert.ToBoolean(await ValidateGuestLogin(guestRoomMappingVM));
            if (result)
            {
                var tokenString = GenerateGuestJWT(guestRoomMappingVM);
                _guestroomrepository = new GuestRoomMappingRepository(_context);
                _hotelRepository = new HotelRepository(_context);
                _guestRepository = new HotelGuestRepository(_context);
                _roomRepository = new HotelRoomRepository(_context);
                List<GuestRoomMapping> GuestRoomMappings = await _guestroomrepository.GetbyFilterAsync(a =>
                          a.RoomId == guestRoomMappingVM.RoomId &&
                          a.CheckIn <= DateTime.Now &&
                          a.CheckOut >= DateTime.Now).Result.ToListAsync();
                GuestRoomMappingVM GuestRoomMappingVM1 = _mapper.Map<GuestRoomMappingVM>(GuestRoomMappings.LastOrDefault());
                HotelRoom hotelRoom = await _roomRepository.GetbyFilterAsync(a => a.Id == GuestRoomMappingVM1.RoomId).Result.Include(a => a.Labels).FirstOrDefaultAsync();
                GuestRoomMappingVM1.Room = _mapper.Map<HotelRoomVM>(hotelRoom);
                Hotel hotel = await _hotelRepository.GetbyFilterAsync(a => a.Id == hotelRoom.HotelId).Result.Include(a => a.Labels).FirstOrDefaultAsync();
                GuestRoomMappingVM1.Hotel = _mapper.Map<HotelVM>(hotel);
                HotelGuest hotelguest = _guestRepository.GetAsync(GuestRoomMappingVM1.GuestId).Result;
                GuestRoomMappingVM1.Guest = _mapper.Map<HotelGuestVM>(hotelguest);
                GuestRoomMappingVM1.Password = "";
                GuestRoomMappingVM1.PasswordSalt = "";
                return Ok(new { token = tokenString, GuestRoomMapping = GuestRoomMappingVM1});
            }
            else
            {
                return Unauthorized();
            }
        }
        [Route("api/UnSubscribe")]
        [HttpPost]
        public async Task<IActionResult> UnSubscribe([FromBody] PushSubscriptionDto subscriptionDto)
        {
            try
            {
                var subscription = new PushSubscription(subscriptionDto.subscription.Endpoint,
               subscriptionDto.subscription.Keys.P256dh, subscriptionDto.subscription.Keys.Auth);
                PushNotificationVM hotelPushNotificationSubcriptionsVM = new PushNotificationVM();
                hotelPushNotificationSubcriptionsVM.Endpoint = subscriptionDto.subscription.Endpoint;
                hotelPushNotificationSubcriptionsVM.Auth = subscriptionDto.subscription.Keys.Auth;
                hotelPushNotificationSubcriptionsVM.clientId = subscriptionDto.clientId;
                hotelPushNotificationSubcriptionsVM.P256dh = subscriptionDto.subscription.Keys.P256dh;
                hotelPushNotificationSubcriptionsVM.Device_id = subscriptionDto.deviceId;
                hotelPushNotificationSubcriptionsVM.userId = subscriptionDto.userId;

                var previousResult = _context.PushNotification.Where(x => x.Endpoint == hotelPushNotificationSubcriptionsVM.Endpoint && x.Auth == hotelPushNotificationSubcriptionsVM.Auth
                && x.P256dh == hotelPushNotificationSubcriptionsVM.P256dh);
                foreach (var v in previousResult)
                {
                    _context.PushNotification.Remove(v);
                }
                await _context.SaveChangesAsync();



                Console.WriteLine($"Unsubscription: {subscriptionDto.subscription.Endpoint}");

                // Respond with a 200 OK status
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        private async Task<bool> ValidateGuestLogin(GuestRoomMappingVM guestRoomMappingVM)
        {
            bool result = Convert.ToBoolean(await CheckGuestLogin(guestRoomMappingVM));
            if (result)
            {
                return true;
            }
            else
            {
                return false;
            }

            
        }

        private async Task<bool> CheckGuestLogin(GuestRoomMappingVM guestRoomMappingVM)
        {
            bool result = false;
            _guestroomrepository = new GuestRoomMappingRepository(_context);
            try
            {

                List<GuestRoomMapping> GuestRoomMappings = await _guestroomrepository.GetbyFilterAsync(a =>
                a.RoomId == guestRoomMappingVM.RoomId &&
                a.CheckIn <= DateTime.Now &&
                a.CheckOut >= DateTime.Now && 
                !a.IsFinished).Result.ToListAsync();


                GuestRoomMapping lastguestroom = GuestRoomMappings.LastOrDefault();

                //check user
                if (lastguestroom != null)
                {
                    // byte[] passwordHashBytes = Convert.FromBase64String(lastguestroom.Password);
                    // byte[] passwordSaltBytes = Convert.FromBase64String(lastguestroom.PasswordSalt);
                    // passwordSaltBytes = Convert.FromBase64String(user.PasswordSalt);
                    // result = PasswordHasher.VerifyPassword(guestRoomMappingVM.Password, passwordSaltBytes, passwordHashBytes);
                    result = guestRoomMappingVM.Password == lastguestroom.Password;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        private string GenerateGuestJWT(GuestRoomMappingVM guestRoomMappingVM)
        {
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var expiry = guestRoomMappingVM.CheckOut;
            var securityKey = new SymmetricSecurityKey
        (Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials
        (securityKey, SecurityAlgorithms.HmacSha256);
            
            List<Claim> claims;
            
                claims = new List<Claim>() {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                         new Claim(ClaimTypes.Role, "Customer")
                    };
            


            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiry,
                signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            var stringToken = tokenHandler.WriteToken(token);
            return stringToken;
        }

    }
}
