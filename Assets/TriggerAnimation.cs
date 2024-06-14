using UnityEngine;

public class TriggerAnimation : MonoBehaviour
{
    [SerializeField] private string triggerName;
    
    private Animator _animator;
    private AudioSource _audioSource;
    
    void Start()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }
    
    public void Animate()
    {
        _animator.SetTrigger(triggerName);
        _audioSource.Play();
    }
}
