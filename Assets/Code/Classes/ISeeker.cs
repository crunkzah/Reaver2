

public interface ISeeker
{
    
    void Register();
    void Unregister();
    void NoticeTarget(UnityEngine.Vector3 targetPos);
    float ViewDistance { get; }
}
