using UnityEngine;

[CreateAssetMenu(fileName = "New Monologue", menuName = "Dialogues/New Monologue")]
public class Monologue : ScriptableObject
{
    public DialogueAvatar avatar;
    public string[] lines_eng;
    public string[] lines_rus;
    
    string GetLineEnglish(int line_index)
    {
        if(line_index >= lines_eng.Length)
        {
            return "Line not found Line not found Line not found Line not found Line not found";
        }
        
        return lines_eng[line_index];
    }
    
    string GetLineRussian(int line_index)
    {
        if(line_index >= lines_eng.Length)
        {
            return "Реплика не найдена Реплика не найдена Реплика не найдена";
        }
        
        return lines_rus[line_index];
    }
    
    // public string Name_eng = "NAME";
    // public string Name_ru = "ИМЯ";
    
    // public string GetName()
    // {
    //     switch(UberManager.lang)
    //     {
    //         case(Language.English):
    //         {
    //             return Name_eng;
    //         }
    //         case(Language.Russian):
    //         {
    //             return Name_ru;
    //         }
    //         default:
    //         {
    //             return Name_eng;
    //         }
    //     }
    // }
    
    public string GetLine(int line_index)
    {
        switch(UberManager.lang)
        {
            case(Language.English):
            {
                return GetLineEnglish(line_index);
            }
            case(Language.Russian):
            {
                return GetLineRussian(line_index);
            }
            default:
            {
                return GetLineEnglish(line_index);
            }
        }
    }
}
