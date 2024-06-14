using System;

namespace LegacyApp
{
    public class UserService
    {
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
    {
        if (!IsValidUserData(firstName, lastName, email, dateOfBirth))
        {
            return false;
        }

        var client = new ClientRepository().GetById(clientId);
        var user = CreateUser(firstName, lastName, email, dateOfBirth, client);

        SetUserCreditLimit(user, client);

        if (user.HasCreditLimit && user.CreditLimit < 500)
        {
            return false;
        }

        UserDataAccess.AddUser(user);
        return true;
    }

    private bool IsValidUserData(string firstName, string lastName, string email, DateTime dateOfBirth)
    {
        return !(string.IsNullOrEmpty(firstName) ||
                 string.IsNullOrEmpty(lastName) ||
                 !email.Contains("@") ||
                 !email.Contains(".") ||
                 GetAge(dateOfBirth) < 21);
    }

    private int GetAge(DateTime dateOfBirth)
    {
        var now = DateTime.Now;
        int age = now.Year - dateOfBirth.Year;
        if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
        {
            age--;
        }
        return age;
    }

    private User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, Client client)
    {
        return new User
        {
            Client = client,
            DateOfBirth = dateOfBirth,
            EmailAddress = email,
            FirstName = firstName,
            LastName = lastName
        };
    }

    private void SetUserCreditLimit(User user, Client client)
    {
        if (client.Type == "VeryImportantClient")
        {
            user.HasCreditLimit = false;
        }
        else
        {
            user.HasCreditLimit = true;
            using (var userCreditService = new UserCreditService())
            {
                int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                if (client.Type == "ImportantClient")
                {
                    creditLimit *= 2;
                }
                user.CreditLimit = creditLimit;
            }
        }
    }
        
    }
}
