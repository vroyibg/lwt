namespace Lwt.Test.Services
{
    using System;
    using System.Threading.Tasks;
    using Lwt.Exceptions;
    using Lwt.Interfaces;
    using Lwt.Models;
    using Lwt.Services;
    using Microsoft.AspNetCore.Identity;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserServiceTest.
    /// </summary>
    public class UserServiceTest
    {
        private readonly UserService userService;

        private readonly Mock<UserManager<User>> userManager;
        private readonly Mock<IUserPasswordChanger> userPasswordChangerMock;
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly Mock<IUserSettingRepository> userSettingRepository;
        private readonly Mock<IMapper<UserSetting, UserSettingView>> userSettingViewMapper;
        private readonly Mock<IMapper<User, UserView>> userViewMapper;
        private readonly Mock<IMapper<UserSettingUpdate, UserSetting>> userSettingUpdateMapper;

        private readonly Mock<ITokenProvider> tokenProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserServiceTest"/> class.
        /// a.
        /// </summary>
        public UserServiceTest()
        {
            var userStore = new Mock<IUserStore<User>>();

            this.userRepositoryMock = new Mock<IUserRepository>();
            this.userViewMapper = new Mock<IMapper<User, UserView>>();
            this.userSettingRepository = new Mock<IUserSettingRepository>();
            this.userSettingViewMapper = new Mock<IMapper<UserSetting, UserSettingView>>();
            this.userSettingUpdateMapper = new Mock<IMapper<UserSettingUpdate, UserSetting>>();
            this.userPasswordChangerMock = new Mock<IUserPasswordChanger>();
            this.userManager = new Mock<UserManager<User>>(
                userStore.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            this.tokenProvider = new Mock<ITokenProvider>();
            this.userService = new UserService(
                this.userManager.Object,
                this.tokenProvider.Object,
                this.userViewMapper.Object,
                this.userSettingRepository.Object,
                this.userSettingViewMapper.Object,
                this.userSettingUpdateMapper.Object,
                this.userPasswordChangerMock.Object,
                this.userRepositoryMock.Object);
        }

        /// <summary>
        /// test login throw bad request if user does not exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task LoginAsyncShouldThrowBadRequestIfUserNotExist()
        {
            // arrange
            var userName = "userName";
            var password = "password";
#nullable disable
            this.userManager.Setup(m => m.FindByNameAsync(userName)).ReturnsAsync((User)null);
#nullable disable

            // act

            // assert
            await Assert.ThrowsAsync<BadRequestException>(() => this.userService.LoginAsync(userName, password));
        }

        /// <summary>
        /// login should throw bad request if the password is wrong.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task LoginAsyncShouldThrowBadRequestIfWrongPassword()
        {
            // arrange
            var userName = "userName";
            var password = "password";
            var user = new User();
            this.userManager.Setup(m => m.FindByNameAsync(userName)).ReturnsAsync(user);
            this.userManager.Setup(m => m.CheckPasswordAsync(user, password)).ReturnsAsync(false);

            // assert
            await Assert.ThrowsAsync<BadRequestException>(() => this.userService.LoginAsync(userName, password));
        }

        /// <summary>
        /// login should return authentication token if success.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task LoginAsyncShouldReturnTokenIfSuccess()
        {
            // arrange
            var userName = "user";
            var password = "pass";
            var user = new User();
            string token = Guid.NewGuid().ToString();
            this.userManager.Setup(m => m.FindByNameAsync(userName)).ReturnsAsync(user);
            this.userManager.Setup(m => m.CheckPasswordAsync(user, password)).ReturnsAsync(true);
            this.tokenProvider.Setup(p => p.GenerateUserToken(user)).Returns(token);

            // act
            string actual = await this.userService.LoginAsync(userName, password);

            // assert
            Assert.Equal(token, actual);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SignUpShouldCallUserManageCreateOnce()
        {
            // arrange
            this.userManager.Reset();
            this.userManager.Setup(m => m.CreateAsync(It.IsAny<User>(), "pass")).ReturnsAsync(IdentityResult.Success);

            // act
            await this.userService.SignUpAsync("user", "pass");

            // assert
            this.userManager.Verify(m => m.CreateAsync(It.IsAny<User>(), "pass"), Times.Once);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SignUpShouldReturnNotThrowIfCreateUserSuccess()
        {
            // arrange
            this.userManager.Reset();
            var userName = "user";
            var password = "pass";

            this.userManager.Setup(m => m.CreateAsync(It.Is<User>(u => u.UserName == userName), password))
                .ReturnsAsync(IdentityResult.Success);

            // act
            await this.userService.SignUpAsync(userName, password);
        }

        /// <summary>
        /// a.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SignUpShouldThrowExceptionIfFailToCreateUser()
        {
            // arrange
            this.userManager.Reset();
            var userName = "user";
            var password = "pass";

            this.userManager.Setup(m => m.CreateAsync(It.Is<User>(u => u.UserName == userName), password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Hello" }));

            // act

            // assert
            await Assert.ThrowsAsync<BadRequestException>(() => this.userService.SignUpAsync(userName, password));
        }

        /// <summary>
        /// test user not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsyncShouldThrowNotFoundIfUserNotFound()
        {
            Guid userId = Guid.NewGuid();
            this.userManager.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync((User)null);

            await Assert.ThrowsAsync<NotFoundException>(() => this.userService.GetAsync(userId));
        }

        /// <summary>
        /// user should be mapped before return.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsyncShouldMapUser()
        {
            Guid userId = Guid.NewGuid();
            var user = new User();
            var userViewModel = new UserView();
            this.userManager.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            this.userViewMapper.Setup(m => m.Map(user)).Returns(userViewModel);

            UserView result = await this.userService.GetAsync(userId);
            Assert.Equal(userViewModel, result);
        }

        /// <summary>
        /// test user setting not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetSettingAsyncShouldThrowNotFoundIfUserNotFound()
        {
            Guid userId = Guid.NewGuid();
            this.userSettingRepository.Setup(r => r.TryGetByUserIdAsync(userId)).ReturnsAsync((UserSetting)null);

            await Assert.ThrowsAsync<NotFoundException>(() => this.userService.GetSettingAsync(userId));
        }

        /// <summary>
        /// user should be mapped before return.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetSettingAsyncShouldMap()
        {
            Guid userId = Guid.NewGuid();
            var userSettingView = new UserSettingView();
            var userSetting = new UserSetting();
            this.userSettingRepository.Setup(r => r.TryGetByUserIdAsync(userId)).ReturnsAsync(userSetting);
            this.userSettingViewMapper.Setup(m => m.Map(userSetting)).Returns(userSettingView);

            UserSettingView result = await this.userService.GetSettingAsync(userId);
            Assert.Equal(userSettingView, result);
        }

        /// <summary>
        /// if the user that changing password is not exist throw not found exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ChangePasswordAsyncShouldThrowNotFoundIfUserNotFound()
        {
            Guid userId = Guid.NewGuid();
            this.userManager.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync((User)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => this.userService.ChangePasswordAsync(userId, It.IsAny<UserChangePasswordModel>()));
        }

        /// <summary>
        /// if fail to change password throw bad request exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ChangePasswordAsyncShouldThrowBadRequestIfFailToChangePassword()
        {
            this.userPasswordChangerMock
                .Setup(c => c.ChangePasswordAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);
            this.userRepositoryMock.Setup(r => r.IsExistAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            await Assert.ThrowsAsync<BadRequestException>(
                () => this.userService.ChangePasswordAsync(Guid.NewGuid(), new UserChangePasswordModel()));
        }

        /// <summary>
        /// should create new user setting if not existed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PutSettingAsyncShouldCreateSettingIfNotExisted()
        {
            Guid userId = Guid.NewGuid();
            this.userSettingRepository.Setup(r => r.TryGetByUserIdAsync(userId)).ReturnsAsync((UserSetting)null);
            this.userSettingUpdateMapper.Setup(m => m.Map(It.IsAny<UserSettingUpdate>())).Returns(new UserSetting());

            await this.userService.PutSettingAsync(userId, new UserSettingUpdate());

            this.userSettingRepository.Verify(r => r.AddAsync(It.Is<UserSetting>(s => s.UserId == userId)));
        }

        /// <summary>
        /// put setting async should update user setting.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PutSettingAsyncShouldUpdateUserSetting()
        {
            Guid userId = Guid.NewGuid();
            var setting = new UserSetting();
            this.userSettingRepository.Setup(r => r.TryGetByUserIdAsync(userId)).ReturnsAsync(new UserSetting());
            this.userSettingUpdateMapper
                .Setup(m => m.Map(It.IsAny<UserSettingUpdate>(), It.IsAny<UserSetting>()))
                .Returns(setting);

            await this.userService.PutSettingAsync(userId, new UserSettingUpdate());

            this.userSettingRepository.Verify(r => r.UpdateAsync(setting));
        }
    }
}