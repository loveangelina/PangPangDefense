using UnityEngine;

public struct BoardItem
{
    public SpriteRenderer spriteRenderer; // 스프라이트 렌더러
    public int index;
    // BoardItem 정보를 갖고있으면 index가 있지만
    // 게임오브젝트에서 boarditem 정보를 얻는 방법이 없으므로 딕셔너리 사용 
    
    public BoardItem(SpriteRenderer spriteRenderer, int index)
    {
        this.spriteRenderer = spriteRenderer;
        this.index = index;
    }
}
