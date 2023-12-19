namespace SmallLister.Web.Handlers.RequestResponse;

public class SigninResponse(bool isReturningUser, string verifyOptions)
{
    public bool IsReturningUser { get; } = isReturningUser;
    public string VerifyOptions { get; } = verifyOptions;
}