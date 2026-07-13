using Cysharp.Threading.Tasks;

public interface IIntroStep
{
    void Prepare(GameIntroConfig config);
    UniTask Play(GameIntroConfig config);
}
