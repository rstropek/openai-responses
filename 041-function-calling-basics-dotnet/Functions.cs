using OpenAI.Responses;

namespace FunctionCallingBasics;

public record BuildPasswordParameters(int MinimumPasswordLength);

public record Password(string PasswordValue);

public static class PasswordFunctions
{
    public static readonly ResponseTool BuildPasswordTool = ResponseTool.CreateFunctionTool(
        functionName: nameof(BuildPasswordTool),
        functionDescription: "Generates an easy to remember password by concatenating a random word from the list of frequent words with a random number.",
        functionParameters: BinaryData.FromBytes("""
        {
            "type": "object",
            "properties": {
                "minimumPasswordLength": {
                    "type": "integer",
                    "description": "Minimum length of the password. Set to 0 for default length (15 characters)"
                }
            },
            "required": ["minimumPasswordLength"],
            "additionalProperties": false
        }
        """u8.ToArray()),
        functionSchemaIsStrict: true
    );

    public static string BuildPassword(BuildPasswordParameters parameters)
    {
        var random = new Random();

        string GetRandomWord()
        {
            return FrequentWordList.FrequentWords[random.Next(FrequentWordList.FrequentWords.Length)];
        }

        string password = "";
        while (password.Length < parameters.MinimumPasswordLength)
        {
            string nextWord = GetRandomWord();
            if (password.Length > 0)
            {
                password += char.ToUpper(nextWord[0]) + nextWord.Substring(1);
            }
            else
            {
                password += nextWord;
            }
        }

        return password;
    }
}
