1. Coding Convention
1-a. Naming Convention 및 줄바꿈
1-a-(1). 아래와 같은 기능을 담당하는 메서드의 경우, 아래와 같은 형식의 메서드 작명 규칙을 준수해야 한다. 적절한 메서드명이 떠오르지 않을 경우, PR에 해당 내용을 추가한다.
-      기능 : 초기화
-      메서드명 : Init + [초기화 대상]
-      예시 : InitPlayerData(), InitMapInfo()
 
-      기능 : 변경(이미 초기화된 대상에 대해 내부 데이터가 변화하거나 다른 대상을 대입할 때)
-      메서드명 : Set + [변경 대상]
-      예시 : SetQuest(), SetAdventurerData()
 
-      기능 : UI에 데이터 반영
-      메서드명 : Refresh + [대상](필요에 따라 생략 가능)
-      예시 : RefreshAdventurerCard(), RefreshQuest()
 
1-a-(2). If문 내부의 로직이 단순 return과 같은 1줄의 형태더라도, 항상 중괄호를 연다.

1-a-(4). C# 제네릭 컬렉션의 컨테이너를 사용할 땐, 항상 필드명에 해당 컨테이너명을 접미어로 붙여준다. Dictionary의 접미어는 Dict으로 축약하여 작성한다. 

1-a-(5). 인터페이스 추가 시, 인터페이스명에 접두사 I를 명시한다. 

1-a-(6). 열거형(enum) 추가 시, 열거형명에 접두사 E를 명시한다. 

1-a-(7). UI 로직을 수행하는 클래스 추가 시, 클래스명에 접두사 UI_를 명시한다.

1-a-(8). 메서드를 선언한 후에 여는 중괄호는 새로운 줄에 추가한다.


1-b. 필드 및 프로퍼티
1-b-(1). 클래스 이름, 함수 이름, Enum 이름, public 필드는 PascalCase로 작성한다.
private, protected, internal, protected internal 필드는 _ + camelCase로 작성한다.
지역 변수, 매개변수는 camelCase로 작성한다.

각 케이스에서 대소문자 구별을 위한 ‘단어’는 여러 단어로 이루어진 이름이나 문구의 첫 글자를 모아 만든 단어인 두문자어를 포함하여 내부 공백 없이 쓰여진 모든 것을 의미한다.
예를 들어 Remote Procedure call을 줄인 rpc를 포함한 단어일 경우 MyRPC가 아니라 MyRpc로 명명한다.

1-b-(2). 클래스 내의 필드는 최대한 public 대신 private로 선언한다. 인스펙터에 노출이 필요한 필드는 [SerializeField] + private으로 선언한다.

1-b-(3). 필드를 선언할 때, [SerializeField] 속성을 선언해줄 경우, 선언부를 한줄에 작성하지 않고, [SerializeField] 작성 후 줄바꿈을 진행한다.

1-b-(4). 단, Serializeable 클래스와 Scirptable Object(SO) 클래스, DTO(Data Transfer Object) 클래스에 한해 public 필드를 선언할 수 있다.

1-b-(5). 대리자(delegate, Action, Func 등) 필드는, 항상 public 필드로 선언한다. 1-f에서 자세히 설명한다.

1-b-(6). 필드를 클래스 외부에서 접근해야 하는 경우에만 프로퍼티를 선언한다. 즉, 프로퍼티의 선언은 필수가 아니다. 프로퍼티 접근자 선언 방식은 별도의 제한을 두지 않는다.

1-b-(7). 필드와 프로퍼티는 클래스 최상단에 선언하고, 프로퍼티는 필드의 바로 아랫줄에 선언한다.
나쁜 예시)
[SerializeField] private float _startScale = 1f;
[SerializeField] private float _endScale = 1.2f;
[SerializeField] private float _duration = 0.5f;

public float StartScale { get => _startScale; set => _startScale = value; }
public float EndScale { get => _endScale; set => _endScale = value; }
public float Duration { get => _duration; set => _duration = value; }
좋은 예시)
[SerializeField] 
private float _startScale = 1f;
public float StartScale { get => _startScale; set => _startScale = value; }
[SerializeField] 
private float _endScale = 1.2f;
public float EndScale { get => _endScale; set => _endScale = value; }
[SerializeField] 
private float _duration = 0.5f;
public float Duration { get => _duration; set => _duration = value; }

1-b-(8). 변수에 한해서, public -> protected -> private 순서로 작성한다. 프로퍼티 작성의 경우 private 순서에 작성한다.

1-c. 이벤트 메서드(생명 주기 메서드) 선언
1-c-(1). 이벤트 메서드에는 반드시 접근 제한자를 선언한다. 생략해서는 안된다.
나쁜 예시)
void Awake()
{
}
좋은 예시)
protected override void Awake()
{
	base.Awake();
}

private void Start()
{
}

1-c-(2). Monobehaviour 혹은 유니티 이벤트 인터페이스를 상속하는 스크립트의 경우, Unity 이벤트 메서드(생명 주기 메서드)를 일반 메서드보다 위쪽에 작성한다. 즉, 일반 메서드 밑에 유니티 이벤트 메서드가 선언되어선 안된다.
나쁜 예시)
public class Test : Monobehaviour
{
	[SerializeField]
	private float _fieldA;
	[SerializeField]
	private float _fieldB;

	private void MethodA()
	{
	}
	
	private float MethodB()
	{
		return _fieldA + _fieldB;
	}
	private float Start()
	{
	}
	private float Awake()
	{
	}
좋은 예시)
public class Test : Monobehaviour
{
	[SerializeField]
	private float _fieldA;
	[SerializeField]
	private float _fieldB;

	private float Awake()
	{
	}
	private float Start()
	{
	}
	private void MethodA()
	{
	}
	private float MethodB()
	{
		return _fieldA + _fieldB;
	}

1-c-(3). 이벤트 메서드의 경우, 스크립트 상단에서부터
----------------------------------------------------
초기화부(Awake(), OnEnable(), Start() 등)
게임 로직부(Update(), FixedUpdate(), 충돌계열 이벤트 메서드 등)
해체부(OnApplicationQuit(), OnDisable(), OnDestroy())
----------------------------------------------------
순으로 작성한다. 예를 들어, OnDestroy() 메서드가 Start() 메서드의 상단에 위치해서는 안된다.
나쁜 예시)
private void Update()
{
}
private void Awake()
{
}
private void OnDestroy()
{
}
private void Start()
{
}
좋은 예시)
private void Awake()
{
}
private void Start()
{
}
private void Update()
{
}
private void FixedUpdate()
{
}
private void OnTriggerEnter2D(Collider2D other)
{
}
private void OnDestroy()
{
}


1-d. null 체크
1-d-(1). 스크립트 내에서 객체의 null 체크를 진행할 땐, 아래와 같은 방식으로 진행한다.
순수 C# 객체(POCO)의 경우, is 또는 ReferenceEquals()를 통해 null 체크를 진행한다. 
Unity 객체의 경우, 상황에 따라 다양한 null 체크 방식을 사용한다.
1-e. 싱글톤
1-e-(1). 싱글톤 클래스의 경우 POCO 객체(순수 C# 객체) / MonoBehaviour 상속 Unity 객체에 따라 POCOSingleton<T> / MonoBehaviourSingleton<T> 제네릭 클래스를 상속하여 사용한다.

1-e-(2). MonoBehaviourSingleton<T>를 상속하는 싱글톤 클래스의 경우,, Awake 메서드의 오버라이딩 버전을 파생 클래스에 선언한 후, base.Awake()를 호출해준다.
 
1-e-(3). 클래스의 해체부 이벤트 메서드(OnDestroy, OnApplicationQuit)에서는 싱글톤 클래스 인스턴스에 접근하는 것을 지양한다. 대신, 객체를 파괴해야할 때 별도 메서드를 호출하여, 해당 메서드 내에서 접근하는 것을 권장한다.
나쁜 예시)
public void OnDestroy()
{
	CurrencyManager.Instance.Get(0);
	GameManager.Instance.PlayData.Score += 100;
}
좋은 예시)
public void OnDead()
{
	CurrencyManager.Instance.Get(0);
	GameManager.Instance.PlayData.Score += 100;
	Destroy(gameObject);
}


1-f. 대리자
1-f-(1). 프로젝트 내에서 사용하는 대리자 필드의 경우, 항상 public 접근 제한자를 가진 PascalCase로 작성한다.

1-f-(2). 대리자 필드명은 항상 접두사 “On”을 사용한다.

1-f-(3). 대리자 필드는 클래스 최상단에 작성한다. 해당 규칙은 필드, 프로퍼티보다 우선순위가 높다.(즉, 일반 필드 및 프로퍼티보다 상단에 대리자 필드가 선언되어야 한다.)

1-f-(4). 대리자 필드명은 항상 접두사 “On”을 사용한다.

1-f-(5). GameObjct 활성화/비활성화 시 이벤트 처리는 OnEnable / OnDisable을 활용한다.
GameObject.SetActive(false)로 비활성화될 때 구독을 해제하고, SetActive(true)로 활성화될 때 구독을 다시 설정하는 로직(예: 대리자/이벤트 등록 및 해제)은 **OnEnable / OnDisable**에 작성한다. 

1-f-(6). Action의 경우 event 키워드를 포함하여 public event Action의 형태로 작성한다.
예시)
public event Action OnPickUp;

1-g. 스크립트 파일 분리
1-g-(1). interface, enum, class는 모두 별도의 파일(스크립트)로 분리한다. 즉, 하나의 스크립트 파일에 2개 이상의 클래스나 인터페이스, enum 등이 존재해선 안된다.
나쁜 예시)

좋은 예시)


1-g-(2). 파일(스크립트)명과 해당 클래스/인터페이스/열거형/구조체 의 이름은 동일해야 한다.


1- h. 주석
1-h-(1). 잘못된 코드를 대체할 목적으로 주석을 추가하지 않는다. 적합한 이름이 부여된 클래스, 변수 또는 메서드는 주석을 대신한다.

1-h-(2). 대부분의 상황에서 이중 슬래시( // ) 주석 태그를 사용한다.

1-h-(3). [SerializeField]가 부착된 필드에는 주석 대신 툴팁을 사용한다.



1- i. 매직 넘버 및 문자열 방지
1-i-(1). 코드 내에 의미를 알 수 없는 숫자나 문자열을 직접 사용하지 않고, 반드시 변수로 정의하여 사용한다. 상수의 경우 const를 사용한다.
나쁜 예시)
transform.posiion.x = 5.0f;
좋은 예시)
private const float _xPosition = 5.0f;
...
transform.position.x = _xPosition;

1-i-(2). 빈 문자열을 표현할 때는 “”대신 “string.Empty”를 사용한다.
if (text == string.Empty)


1- j. var 키워드 사용 규칙
1-j-(1). var 사용 권장 경우 : 변수의 타입이 선언부에서 명확하게 파악 가능할 경우, var 키워드 사용을 권장한다. (예: new List<string>()을 할당할 때)

1-j-(2). var 사용 금지 경우 : 메서드 반환 값이나 숫자 리터럴처럼 타입 추론이 모호할 경우, 명시적인 타입을 사용한다. (예: int, float같은 리터럴은 명시적인 타입으로 작성)


1- k. 코루틴 사용 금지
1-k-(1). 코루틴을 사용하지 않는다. 대신 UniTask를 사용한다. 2-c에서 자세히 설명한다.


1- l. 비동기(Async/Await) 규칙
1-l-(1). 비동기 메서드는 반드시 접미사 Async를 사용한다. (예: LoadDataAsync)

1-l-(2). 비동기 메서드는 UniTask 또는 UniTask<T>를 반환해야 한다. async void는 사용을 금지하고, Unity의 이벤트 핸들러와 같이 await가 필요 없고 반환값이 없는 경우 UniTaskVoid를, await는 필요하지만 반환값이 없는 경우 UniTask를 사용한다.


1- m. ReadOnly Collections
1-m-(1). 변경을 제한하고 싶은 컬렉션에 readonly 키워드만 추가하면 새로 생성하는 것만 불가능할 뿐 기존 컬렉션에 Add / Remove는 가능하다. 따라서 런타임에 Add / Remove가 되면 안되는 Dictionary와 List가 있다면 ReadOnlyDictionary<T>, ReadOnlyList<T> 클래스를 사용한다.
// Add / Remove 가능
private readonly List<int> numList;
private readonly Dictionary<int, int> numDict;

// Add / Remove 불가 (처음 할당 후 변경 불가)
private ReadOnlyList<int> numList;
private ReadOnlyDictionary<int, int> numDict;


2. External Packages
2- a. Dictionary 대신 SerializedDictionary 사용 경우
2-a-(1). 내부에서만 사용되는 Dictionary가 아닌 경우 (인스펙터에서 보거나 수정하는 등의 테스트가 필요한 경우) SerializedDictionary를 사용한다.

2-a-(2). 이때 사용하는 SerializedDictionary의 경우 UnityEngine.Rendering 네임스페이스가 아닌 VInspector의 SerializedDictionary를 사용한다.
using VInspector;

[SerializeField]
public SerializedDictionary<EStatType, Stat> StatDictionary;



2-b. Inspector 가독성
2-b-(1). 에디터의 Inspector에서 직접 할당해줘야 하는 [SerializeField] 혹은 public 필드의 경우, 할당 위치에 따라 VInspector의 FoldOut 어트리뷰트를 추가해준다. 이후 필드의 분류는 Header를 통해 진행한다.

2- c. UniTask 사용 규칙
2-c-(1). 메인스레드 기본 실행 원칙
모든 비동기 로직은 특별한 지시가 없는 한 ‘메인 스레드’에서 실행되는 것을 기본으로 한다.
메인 스레드 전환 (명시적 Wait) : 특정 작업의 실행이 끝날 때까지 기다렸다가 다시 메인 스레드에서 실행을 재개해야 할 경우 await UniTask.SwitchToMainThread()를 사용한다.
프레임 루프 타이밍 : Update, LateUpdate, FixedUpdate 등 Unity의 프레임 루프 타이밍에 맞춰 비동기 작업을 재개해야 할 경우 UniTask.Yield(PlayerLoopTiming.DesiredTiming)을 명시하여 사용한다. (예: await UniTask.Yield(PlayerLoopTiming.LateUpdate))

2-c-(2). 백그라운드 스레드 사용 (성능 최적화)
CPU 부하가 크고 Unity API에 접근하지 않는 순수 C# 작업(예: 복잡한 계산, JSON 파싱, 파일 I/O 등)은 메인 스레드를 막지 않도록 백그라운드 스레드에서 실행해야 한다.
스레드 전환 : 백그라운드 스레드로 작업을 전환할 때는 await UniTask.SwitchToThreadPool() 또는 UniTask.RunOnThreadPool(() => { ... })을 사용한다.
스레드에서 Unity API 접근 금지 : 백그라운드 스레드 내에서는 GameObject, Transform, GetComponent 등 Unity의 API에 절대 접근해서는 안된다. Unity API 접근이 필요한 경우, 반드시 메인 스레드로 복귀해야 한다.
private async UniTaskVoid TestTask()
{
    // 스레드 풀로 전환하여 백그라운드에서 실행
    await UniTask.SwitchToThreadPool();

    // 단순한 계산 작업 (예: 1부터 1000까지의 합 계산)
    int result = 0;
    for (int i = 1; i <= 1000; i++)
    {
        result += i;
    }

    // 메인 스레드로 전환
    await UniTask.SwitchToMainThread();

    // 메인 스레드에서 결과를 출력
    Debug.Log("계산 완료. 최종 결과: " + result);
}

// 예시: 백그라운드에서 계산 후 메인 스레드 복귀
var result = await UniTask.RunOnThreadPool(() => HeavyCalculation(data));
// 이곳은 메인 스레드입니다. 안전하게 Unity API 사용 가능
myTextComponent.text = result.ToString();

2-c-(3). 취소 토큰(CancellationToken) 사용 의무화
장기간 실행되는 비동기 메서드나, 사용자 입력에 의해 중단될 수 있는 비동기 작업에는 ‘CancellationToken’을 반드시 파라미터로 전달하여 작업 취소 메커니즘을 구현해야 한다.
CancellationTokenSource: 취소를 요청하는 주체(예: UI 버튼 클릭, 오브젝트 파괴)는 CancellationTokenSource를 통해 토큰을 생성하고, 작업 중단 시 source.Cancel()을 호출해야 한다.
UniTask의 취소: UniTask는 .WithCancellation(token) 메서드를 통해 취소 토큰을 주입할 수 있으며, await 지점에서 취소가 감지되면 자동으로 작업을 안전하게 종료한다.

2-c-(4). 리턴 타입의 명확한 구분
다음과 같이 비동기 메서드의 리턴 타입을 명확히 구분하여 사용한다.
타입
용도
설명
UniTask<T>
반환 값이 필요한 비동기 작업
작업 완료 후 특정 데이터를 반환해야 할 때 사용.
UniTask
반환 값이 없는 일반 비동기 작업
비동기 호출을 기다려야 하지만, 결과 값은 필요 없을 때 사용.
UniTaskVoid
await가 필요 없는 최상위 비동기 메서드
Unity 이벤트 핸들러(Start, Awake, Button.onClick 등)처럼 호출자가 완료를 기다리지 않는 최상위 메서드에만 사용. (async void의 안전한 대체제)


2-c-(5). MonoBehaviour 라이프사이클을 사용한 종료 관리
MonoBehaviour가 파괴될 때(OnDestroy) 진행 중이던 비동기 작업을 자동으로 취소하여 메모리 누수를 방지한다.
ToUniTask 사용: 기존에 CancellationToken이 명시적으로 없는 상황일 경우, await가 필요한 Unity의 기본 비동기 함수(예: yield return null)를 UniTask로 변환할 때는 this.GetCancellationTokenOnDestroy()를 사용하여 해당 오브젝트 파괴 시 작업을 취소한다.
// 오브젝트 파괴 시 자동으로 해당 Task는 취소됨
await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());


3. Github
3-a. Commit Message Convention
3-a-(1). 기본 구조
커밋 메시지는 태그와 제목, 본문으로 구성한다. 여기서 태그와 제목은 의무적으로 작성한다.
Tag(태그) | Subject(제목)
----------------------
Body(본문)
 
3-a-(2). 태그
태그는 해당 커밋 변경사항의 카테고리를 파악하는 매우 중요한 요소이다.
태그의 종류는 다음과 같다.
Add | 에셋이나 패키지, 이미지 등 단순히 파일을 추가하는 작업
Feat | 새로운 기능 구현한 작업
Fix | 버그를 수정한 작업
Style | 오브젝트나 컴포넌트의 속성값 등을 바꾸는 작업(코드 수정이 없는 경우)
Refactor | 코드 리팩토링 및 기능 개선 작업(기능에 변화가 없는 경우)
Docs | 폴더 정리 및, 파일, 폴더명을 수정하거나 옮기는 작업
Chore | 프로젝트, 빌드 설정의 변경 및 빌드 관련 작업
Remove | 불필요한 파일을 삭제하는 작업
좋은 예시)
Feat | 플레이어 이동 구현
Add | 포션 이미지 추가
Refactor | Ability 캐싱 Dictionary를 이용한 방식으로 변경


 
3-a-(3). Subject(제목)
커밋 메시지의 제목은 50글자 이내로 작성한다.
마침표 및 특수기호는 사용하지 않는다.
현재시제와, 간결한 표현을 사용한다.

3-a-(4). Body(본문)
커밋 메시지의 본문은 의무가 아니므로, 자유롭게 작성한다.
작성한다면, 최대한 상세히(코드 변경의 이유가 명확할수록 좋음) 작성한다. PR 작성시 해당 커밋의 Body가 큰 도움이 된다.

3-b. PR Convention
3-b-(1). 작업한 브랜치를 머지하기 위해서는 PR을 의무적으로 게시해야 한다.
3-b-(2). PR은 Task 단위로 작성한다.
3-b-(3). PR의 경우, PR 게시자를 포함한 최소 2인 이상으로 리뷰를 진행한다.
3-b-(4). PR 제목은, 해당 PR의 커밋 내역을 아우를 수 있는 형식으로 작성한다. PR 제목의 형식은 커밋 메시지 형식과 동일하다.

3-b-(6). PR 게시시, Assignees에 PR 게시자(본인)를 추가한다. Label의 경우 PR 제목의 태그와 해당 태그 이외의 작업을 했을 경우 선택적으로 추가한다. PR 게시자의 이름 태그는 의무적으로 추가한다.
예시)
 
3-b-(7). PR 내용의 경우 기본적으로 레포지토리 내 PR 템플릿을 준수하되, 필요에 따라 카테고리나 항목을 추가적으로 작성한다.

3-b-(8). 리뷰어는 성심성의껏 리뷰를 진행한다. 성의있는 리뷰가 프로젝트 퀄리티 및 PR 게시자와 리뷰어의 실력을 동시에 향상시킬 수 있는 지름길이다.
 
4. Unity Editor
4-a. 폴더 정리
4-a-(1). Unity Editor. 프로젝트 폴더 내 파일들은, 파일 종류에 따라 다음과 같은 케이스를 준수한다.
PascalCase : Scene, 스크립트
예시) GameScene, StartScene, GameManager.cs, CalculateManager.cs
Snake_Case의 변형된 형태 : 프리팹, 이미지(스프라이트), 사운드(오디오클립), 애니메이션 관련 파일
예시) Sprite_Adventurer_1, Prefab_UI_ResultInfo, AudioClip_Item_Drop 등

4-a-(2). 외부에서 import한 에셋 폴더는 반드시 99.External Assets의 하위 폴더로 옮겨둔다.

4-a-(3). 프로젝트 내 폴더 네이밍의 경우, 기능 중심이 아닌 컨텐츠 중심 네이밍 방식을 지향한다. 

4-a-(4). Hierarchy에 존재하는 게임 오브젝트건, Project 폴더 내 Prefab이건, 의미없는 게임 오브젝트명은 절대로 작성하지 않는는다. 이름을 통해 해당 게임 오브젝트가 게임에서 어떤 역할을 하는지 단번에 알 수 있도록 작성한다. 