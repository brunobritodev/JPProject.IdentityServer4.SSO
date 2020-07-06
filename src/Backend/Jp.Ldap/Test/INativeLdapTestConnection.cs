namespace Jp.Ldap.Test
{
    public interface ILdapTestConnection
    {
        LdapConnectionResult Test(string username, string password);
    }
}