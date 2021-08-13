#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemySpawner))]
public class EnemySpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        var style = new GUIStyle(GUI.skin.button);
        GUI.contentColor = Color.yellow;
        GUI.backgroundColor = Color.cyan;
        //style.normal.textColor = Color.yellow;
        
        EnemySpawner t = (EnemySpawner)target;
        
        if(GUILayout.Button("Name Wave", style, GUILayout.Width(150), GUILayout.Height(100)))
        {
            if(t.waves_to_spawn == null)
                return;
            
            int waves_num = t.waves_to_spawn.Length;
            
            for(int i = 0; i < waves_num; i++)
            {
                
                int spawns_len = t.waves_to_spawn[i].singleSpawn.Length;
                for(int j = 0; j < spawns_len; j++)
                {
                    string oldName = t.waves_to_spawn[i].singleSpawn[j].name;
                    if(oldName.Length > 0 && oldName[0] != '_')
                    {
                        string newName = t.waves_to_spawn[i].singleSpawn[j].npc_to_spawn.ToString();
                        newName += "_spawn_W" + (i + 1).ToString() + "_" + (j + 1).ToString();
                        t.waves_to_spawn[i].singleSpawn[j].name = newName;
                        Transform spawn_transform = t.waves_to_spawn[i].singleSpawn[j].pos_tr;
                        if(spawn_transform)
                        {
                            spawn_transform.gameObject.name = newName;    
                            EditorUtility.SetDirty(spawn_transform.gameObject);
                        }
                    }
                    else
                    {
                        Transform spawn_transform = t.waves_to_spawn[i].singleSpawn[j].pos_tr;
                        if(spawn_transform)
                        {
                            spawn_transform.gameObject.name = oldName;    
                            EditorUtility.SetDirty(spawn_transform.gameObject);
                        }
                    }
                }
                
            // //     Debug.Log(string.Format("<color=yellow>Assigned id '<color=green>{0}</color>' for <color=blue>{1}</color></color>", staticLastNetId, net_objs[i].gameObject.name));
            //     net_objs[i].networkId = staticLastNetId;
            //     staticLastNetId++;
                
            //     EditorUtility.SetDirty(net_objs[i]);
            }
            
            EditorUtility.SetDirty(t.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            
        }
        
        
    }
}
#endif