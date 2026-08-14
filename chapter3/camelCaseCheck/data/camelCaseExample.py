"""User account helpers using camelCase naming convention."""


def validateUserCredentials(userName, passWord):
    if not userName or len(userName) < 4 or len(passWord) < 8:
        return False
    hasDigit = any(char.isdigit() for char in passWord)
    hasUpper = any(char.isupper() for char in passWord)
    return hasDigit and hasUpper


def buildUserProfile(firstName, lastName, age):
    return {
        "fullName": f"{firstName} {lastName}",
        "ageInYears": age,
        "isActive": age >= 18,
    }


def resetUserPassword(userId, newPassWord):
    if len(newPassWord) < 8:
        return False
    print(f"Password reset for user #{userId}")
    return True


if __name__ == "__main__":
    print(validateUserCredentials("alice", "Secret123"))
    print(buildUserProfile("Grace", "Hopper", 30))
    print(resetUserPassword(42, "newSecret9"))
