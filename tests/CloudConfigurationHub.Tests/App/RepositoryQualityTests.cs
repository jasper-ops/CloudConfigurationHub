namespace CloudConfigurationHub.Tests.App;

public sealed class RepositoryQualityTests {
    [Fact]
    public void Management_app_does_not_expose_template_demo_pages() {
        var pagesDirectory = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "Pages"));
        var forbiddenTemplatePages = new[] {
            "Counter.razor",
            "Weather.razor",
            "Auth.razor"
        };

        foreach (var page in forbiddenTemplatePages) {
            Assert.False(
                File.Exists(Path.Combine(pagesDirectory, page)),
                $"Template demo page should not be shipped: {page}");
        }
    }

    [Fact]
    public void Non_generated_csharp_files_keep_opening_braces_on_the_previous_line() {
        var repositoryRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            ".."));
        var checkedRoots = new[] {
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.Domain"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.Application"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.Infrastructure"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.Sdk"),
            Path.Combine(repositoryRoot, "tests")
        };
        var violations = checkedRoots
            .SelectMany(root => Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                && !Path.GetFileName(path).EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadLines(path)
                .Select((line, index) => new {
                    Path = Path.GetRelativePath(repositoryRoot, path),
                    LineNumber = index + 1,
                    Line = line
                }))
            .Where(item => item.Line.Trim() == "{")
            .Select(item => $"{item.Path}:{item.LineNumber}")
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void Build_props_keeps_nuget_audit_connectivity_failure_out_of_warnings_as_errors() {
        var repositoryRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            ".."));
        var buildProps = File.ReadAllText(Path.Combine(repositoryRoot, "Directory.Build.props"));

        Assert.Contains("<TreatWarningsAsErrors>true</TreatWarningsAsErrors>", buildProps, StringComparison.Ordinal);
        Assert.Contains("NU1900", buildProps, StringComparison.Ordinal);
    }

    [Fact]
    public void Management_navigation_css_prevents_horizontal_sidebar_scroll() {
        var repositoryRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            ".."));
        var navMenuCss = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "CloudConfigurationHub.App",
            "Components",
            "Layout",
            "NavMenu.razor.css"));

        Assert.Contains("overflow-x: hidden;", navMenuCss, StringComparison.Ordinal);
        Assert.Contains("text-overflow: ellipsis;", navMenuCss, StringComparison.Ordinal);
    }

    [Fact]
    public void Key_identity_management_pages_do_not_ship_template_english_copy() {
        var repositoryRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            ".."));
        var checkedFiles = new[] {
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Login.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Register.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "ExternalLogin.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "LoginWith2fa.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "LoginWithRecoveryCode.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "AccessDenied.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "ConfirmEmail.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "ConfirmEmailChange.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "ForgotPassword.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "ForgotPasswordConfirmation.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "InvalidPasswordReset.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "InvalidUser.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Lockout.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "RegisterConfirmation.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "ResetPassword.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "ResetPasswordConfirmation.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "ResendEmailConfirmation.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Shared", "ExternalLoginPicker.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Shared", "ManageNavMenu.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Shared", "ManageLayout.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Manage", "Index.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Manage", "Email.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Manage", "ChangePassword.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Manage", "TwoFactorAuthentication.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Manage", "ExternalLogins.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Manage", "PersonalData.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Manage", "Passkeys.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Manage", "DeletePersonalData.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Manage", "Disable2fa.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Manage", "EnableAuthenticator.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Manage", "GenerateRecoveryCodes.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Manage", "RenamePasskey.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Manage", "ResetAuthenticator.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Pages", "Manage", "SetPassword.razor"),
            Path.Combine(repositoryRoot, "src", "CloudConfigurationHub.App", "Components", "Account", "Shared", "ShowRecoveryCodes.razor")
        };
        var forbiddenTemplateCopy = new[] {
            ">Log in<",
            "<PageTitle>Log in</PageTitle>",
            ">Use a local account to log in.<",
            "There are no external authentication services configured.",
            ">Create a new account.<",
            ">Profile<",
            ">Phone number<",
            ">Two-factor authentication<",
            ">Manage your account<",
            ">Change your account settings<",
            ">Manage email<",
            ">Send verification email<",
            ">Change email<",
            ">Change password<",
            ">Update password<",
            ">Authenticator app<",
            ">Registered Logins<",
            ">Personal Data<",
            ">Download<",
            ">Manage your passkeys<",
            ">No passkeys are registered.<",
            ">Add a new passkey<",
            ">Associate your ",
            ">Two-factor authentication<",
            ">Authenticator code<",
            "Remember this machine",
            "log in with a recovery code",
            ">Recovery code verification<",
            ">Recovery Code<",
            ">Recovery codes<",
            "Put these codes in a safe place.",
            ">Delete Personal Data<",
            "Delete data and close my account",
            ">Disable two-factor authentication (2FA)<",
            "This action only disables 2FA.",
            ">Configure authenticator app<",
            "To use an authenticator app go through the following steps:",
            ">Generate two-factor authentication (2FA) recovery codes<",
            "Generate Recovery Codes",
            "Enter a name for your passkey",
            ">Passkey name<",
            ">Reset authenticator key<",
            ">Set password<",
            ">Set your password<",
            ">Set password<",
            ">Access denied<",
            ">Confirm email<",
            ">Confirm email change<",
            ">Forgot your password?<",
            ">Forgot password confirmation<",
            ">Invalid password reset<",
            ">Invalid user<",
            ">Locked out<",
            ">Register confirmation<",
            ">Reset password<",
            ">Reset your password.<",
            ">Reset password confirmation<",
            ">Resend email confirmation<",
            ">Reset password<",
            ">Reset<",
            "Please check your email",
            "Click here to confirm your account",
            "Your password has been reset.",
            "Error: Invalid email change confirmation link.",
            "Verification email sent. Please check your email."
        };
        var violations = checkedFiles
            .SelectMany(path => forbiddenTemplateCopy
                .Where(copy => File.ReadAllText(path).Contains(copy, StringComparison.Ordinal))
                .Select(copy => $"{Path.GetRelativePath(repositoryRoot, path)} contains \"{copy}\""))
            .ToArray();

        Assert.Empty(violations);
    }
}
