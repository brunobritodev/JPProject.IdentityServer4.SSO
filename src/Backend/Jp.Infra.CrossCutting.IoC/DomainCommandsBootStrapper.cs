using Jp.Domain.CommandHandlers;
using Jp.Domain.Commands.Role;
using Jp.Domain.Commands.User;
using Jp.Domain.Commands.UserManagement;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Jp.Infra.CrossCutting.IoC
{
    internal class DomainCommandsBootStrapper
    {
        public static void RegisterServices(IServiceCollection services)
        {
            /*
             * Role commands
             */
            services.AddScoped<IRequestHandler<RemoveRoleCommand, bool>, RoleCommandHandler>();
            services.AddScoped<IRequestHandler<RemoveUserFromRoleCommand, bool>, RoleCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateRoleCommand, bool>, RoleCommandHandler>();
            services.AddScoped<IRequestHandler<SaveRoleCommand, bool>, RoleCommandHandler>();

            /*
             * Regiser commands
             */
            services.AddScoped<IRequestHandler<RegisterNewUserCommand, bool>, UserCommandHandler>();
            services.AddScoped<IRequestHandler<RegisterNewUserWithoutPassCommand, bool>, UserCommandHandler>();
            services.AddScoped<IRequestHandler<RegisterNewUserWithProviderCommand, bool>, UserCommandHandler>();
            services.AddScoped<IRequestHandler<SendResetLinkCommand, bool>, UserCommandHandler>();
            services.AddScoped<IRequestHandler<ResetPasswordCommand, bool>, UserCommandHandler>();
            services.AddScoped<IRequestHandler<ConfirmEmailCommand, bool>, UserCommandHandler>();
            services.AddScoped<IRequestHandler<AddLoginCommand, bool>, UserCommandHandler>();


            /*
             * User manager
             */
            services.AddScoped<IRequestHandler<UpdateProfileCommand, bool>, UserManagementCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateProfilePictureCommand, bool>, UserManagementCommandHandler>();
            services.AddScoped<IRequestHandler<SetPasswordCommand, bool>, UserManagementCommandHandler>();
            services.AddScoped<IRequestHandler<ChangePasswordCommand, bool>, UserManagementCommandHandler>();
            services.AddScoped<IRequestHandler<RemoveAccountCommand, bool>, UserManagementCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateUserCommand, bool>, UserManagementCommandHandler>();
            services.AddScoped<IRequestHandler<SaveUserClaimCommand, bool>, UserManagementCommandHandler>();
            services.AddScoped<IRequestHandler<RemoveUserClaimCommand, bool>, UserManagementCommandHandler>();
            services.AddScoped<IRequestHandler<SaveUserRoleCommand, bool>, UserManagementCommandHandler>();
            services.AddScoped<IRequestHandler<RemoveUserRoleCommand, bool>, UserManagementCommandHandler>();
            services.AddScoped<IRequestHandler<AdminChangePasswordCommand, bool>, UserManagementCommandHandler>();
        }
    }
}
