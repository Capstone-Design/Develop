// WindowsProject1.cpp: 응용 프로그램의 진입점을 정의합니다.

#include "stdafx.h"
#include "WindowsProject1.h"
#include  "Winuser.h"
#include  "Shellapi.h"
#include  <vector>

using namespace std;


#define MAX_LOADSTRING 100

// 전역 변수:
HINSTANCE hInst;                                // 현재 인스턴스입니다.
WCHAR szTitle[MAX_LOADSTRING];                  // 제목 표시줄 텍스트입니다.
WCHAR szWindowClass[MAX_LOADSTRING];            // 기본 창 클래스 이름입니다.

// 이 코드 모듈에 들어 있는 함수의 정방향 선언입니다.
ATOM                MyRegisterClass(HINSTANCE hInstance);
BOOL                InitInstance(HINSTANCE, int);
LRESULT CALLBACK    WndProc(HWND, UINT, WPARAM, LPARAM);
INT_PTR CALLBACK    About(HWND, UINT, WPARAM, LPARAM);


// 세팅 클래스 
class SettingValue {
private:
	int screensize[2];             // 화면 실제 크기 mm {가로, 세로}
	int resolution[2];             // 화면 해상도 x, y
	double calibration[2];         // 센서 미세조정값 mm/ pixels [가로, 세로]
	//arduino[2];                  // BLE 연될된 아두이노 2개[좌, 우]
	int firstStart;                // 초기 실행인지 아닌지 확인하는 변수
public:
	//void ResolutionChanged();          
	void SetScreenSize(int* size);
	//void CalculateCalibration(int* size, int* resolution);
	int* GetResolution();
	//int* GetBattery();
	//void SaveToFile();
	//void LoadFromFile();
};

void SettingValue::SetScreenSize(int* size)
{
	screensize[0] = size[0];
	screensize[1] = size[1];
}

int * SettingValue::GetResolution()
{
	resolution[0] = GetSystemMetrics(SM_CXSCREEN);
	resolution[1] = GetSystemMetrics(SM_CYSCREEN);

	return resolution;
}


// 터치 클래스 
class touch {
private:
	//vector<long, int, int> distance;
	//vector<int, int> start;
	POINTER_TOUCH_INFO contact;   //터치 포인터 클래스 
public:
	touch();
	void TouchInput(char type,int x,int y);
	//void Scale(int x, int y);
	//void CalculateTouch();
	//void PutDistance(vector<long, int, int>distance);
};


touch::touch()
{
	if (!InitializeTouchInjection(1, TOUCH_FEEDBACK_DEFAULT))
	{
		//return  FALSE;
	}

	memset(&contact, 0, sizeof(POINTER_TOUCH_INFO));

	// 터치 포인터 개체 값 초기화 
	contact.pointerInfo.pointerType = PT_TOUCH;
	contact.pointerInfo.pointerId = 0;          //contact 0
	contact.pointerInfo.ptPixelLocation.y = 200; // Y 좌표
	contact.pointerInfo.ptPixelLocation.x = 250; // X 좌표

	contact.touchFlags = TOUCH_FLAG_NONE;
	contact.touchMask = TOUCH_MASK_CONTACTAREA | TOUCH_MASK_ORIENTATION | TOUCH_MASK_PRESSURE;
	contact.orientation = 90;
	contact.pressure = 32000;

	// 터치 지점 넓이 설정
	contact.rcContact.top = contact.pointerInfo.ptPixelLocation.y - 2;
	contact.rcContact.bottom = contact.pointerInfo.ptPixelLocation.y + 2;
	contact.rcContact.left = contact.pointerInfo.ptPixelLocation.x - 2;
	contact.rcContact.right = contact.pointerInfo.ptPixelLocation.x + 2;
}

void touch::TouchInput(char type, int x, int y)
{
	switch (type)
	{
	case 'u':
		contact.pointerInfo.pointerFlags = POINTER_FLAG_UP;
		break;
	case 'd':
		contact.pointerInfo.pointerFlags = POINTER_FLAG_DOWN | POINTER_FLAG_INRANGE | POINTER_FLAG_INCONTACT;
		break;
	case 'm':
		contact.pointerInfo.pointerFlags = POINTER_FLAG_UPDATE | POINTER_FLAG_INRANGE | POINTER_FLAG_INCONTACT;
		break;
	default:
		break;
	}

	contact.pointerInfo.ptPixelLocation.x = x; // X 좌표
	contact.pointerInfo.ptPixelLocation.y = y; // Y 좌표

	InjectTouchInput(1, &contact);
}

SettingValue SV; // 속성 클래스 생성
touch TV; 

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
                     _In_opt_ HINSTANCE hPrevInstance,
                     _In_ LPWSTR    lpCmdLine,
                     _In_ int       nCmdShow)
{
    UNREFERENCED_PARAMETER(hPrevInstance);
    UNREFERENCED_PARAMETER(lpCmdLine);

    // TODO: 여기에 코드를 입력합니다.



    // 전역 문자열을 초기화합니다.
    LoadStringW(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);
    LoadStringW(hInstance, IDC_WINDOWSPROJECT1, szWindowClass, MAX_LOADSTRING);
    MyRegisterClass(hInstance);

    // 응용 프로그램 초기화를 수행합니다.
    if (!InitInstance (hInstance, nCmdShow))
    {
        return FALSE;
    }

	////// 터치 기능 초기화 
	////if (!InitializeTouchInjection(1, TOUCH_FEEDBACK_DEFAULT))
	////{
	////	return  FALSE;
	////}
	////
	////memset(&contact, 0, sizeof(POINTER_TOUCH_INFO));


    HACCEL hAccelTable = LoadAccelerators(hInstance, MAKEINTRESOURCE(IDC_WINDOWSPROJECT1));

    MSG msg;
	
	//// 터치 포인터 개체 값 초기화 
	//contact.pointerInfo.pointerType = PT_TOUCH;
	//contact.pointerInfo.pointerId = 0;          //contact 0
	//contact.pointerInfo.ptPixelLocation.y = 200; // Y 좌표
	//contact.pointerInfo.ptPixelLocation.x = 250; // X 좌표

	//contact.touchFlags = TOUCH_FLAG_NONE;
	//contact.touchMask = TOUCH_MASK_CONTACTAREA | TOUCH_MASK_ORIENTATION | TOUCH_MASK_PRESSURE;
	//contact.orientation = 90; 
	//contact.pressure = 32000;

	//// 터치 지점 넓이 설정
	//contact.rcContact.top = contact.pointerInfo.ptPixelLocation.y - 2;
	//contact.rcContact.bottom = contact.pointerInfo.ptPixelLocation.y + 2;
	//contact.rcContact.left = contact.pointerInfo.ptPixelLocation.x - 2;
	//contact.rcContact.right = contact.pointerInfo.ptPixelLocation.x + 2;


	// 터치 입력 개시   :: 드래그 
	//contact.pointerInfo.pointerFlags = POINTER_FLAG_DOWN | POINTER_FLAG_INRANGE | POINTER_FLAG_INCONTACT;
	//InjectTouchInput(1, &contact); // Injecting the touch down on screen

	//contact.pointerInfo.pointerFlags = POINTER_FLAG_UPDATE | POINTER_FLAG_INRANGE | POINTER_FLAG_INCONTACT;
	//for (int i = 0; i < 500; i++)
	//{
	//	contact.pointerInfo.ptPixelLocation.y = contact.pointerInfo.ptPixelLocation.y + 1;
	//	InjectTouchInput(1, &contact);
	//}
	//contact.pointerInfo.pointerFlags = POINTER_FLAG_UP;
	//InjectTouchInput(1, &contact); // Injecting the touch Up from screen
    
    // 기본 메시지 루프입니다.
	TV.TouchInput('d', 200, 250);
	//TV.TouchInput('u', 200, 250);
	int i = 0;
	for (int i = 0; i < 500; i++)
	{
		TV.TouchInput('m', 200, 250 + i);
		
	}
	TV.TouchInput('u', 200, 250 + i);
    while (GetMessage(&msg, nullptr, 0, 0))
    {
        if (!TranslateAccelerator(msg.hwnd, hAccelTable, &msg))
        {
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }
    }

    return (int) msg.wParam;
}



//
//  함수: MyRegisterClass()
//
//  목적: 창 클래스를 등록합니다.
//
ATOM MyRegisterClass(HINSTANCE hInstance)
{
    WNDCLASSEXW wcex;

    wcex.cbSize = sizeof(WNDCLASSEX);

    wcex.style          = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc    = WndProc;
    wcex.cbClsExtra     = 0;
    wcex.cbWndExtra     = 0;
    wcex.hInstance      = hInstance;
    wcex.hIcon          = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_WINDOWSPROJECT1));
    wcex.hCursor        = LoadCursor(nullptr, IDC_ARROW);
    wcex.hbrBackground  = (HBRUSH)(COLOR_WINDOW+1);
    wcex.lpszMenuName   = MAKEINTRESOURCEW(IDC_WINDOWSPROJECT1);
    wcex.lpszClassName  = szWindowClass;
    wcex.hIconSm        = LoadIcon(wcex.hInstance, MAKEINTRESOURCE(IDI_SMALL));

    return RegisterClassExW(&wcex);
}

//
//   함수: InitInstance(HINSTANCE, int)
//
//   목적: 인스턴스 핸들을 저장하고 주 창을 만듭니다.
//
//   설명:
//
//        이 함수를 통해 인스턴스 핸들을 전역 변수에 저장하고
//        주 프로그램 창을 만든 다음 표시합니다.
//


BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
   hInst = hInstance; // 인스턴스 핸들을 전역 변수에 저장합니다.
   // 창 그리기 및 트레이 아이콘 생성
   /*HWND hWnd = CreateWindowW(szWindowClass, szTitle, WS_OVERLAPPEDWINDOW,
      CW_USEDEFAULT, 0, CW_USEDEFAULT, 0, nullptr, nullptr, hInstance, nullptr);

   if (!hWnd)
   {
      return FALSE;
   }
 
   ShowWindow(hWnd, nCmdShow);
   UpdateWindow(hWnd);*/
   

   return TRUE;
}

//
//  함수: WndProc(HWND, UINT, WPARAM, LPARAM)
//
//  목적:  주 창의 메시지를 처리합니다.
//
//  WM_COMMAND  - 응용 프로그램 메뉴를 처리합니다.
//  WM_PAINT    - 주 창을 그립니다.
//  WM_DESTROY  - 종료 메시지를 게시하고 반환합니다.
//
//
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    switch (message)
    {
    case WM_COMMAND:
        {
            int wmId = LOWORD(wParam);
            // 메뉴 선택을 구문 분석합니다.
            switch (wmId)
            {
            case IDM_ABOUT:
                DialogBox(hInst, MAKEINTRESOURCE(IDD_ABOUTBOX), hWnd, About);
                break;
            case IDM_EXIT:
                DestroyWindow(hWnd);
                break;
            default:
                return DefWindowProc(hWnd, message, wParam, lParam);
            }
        }
        break;
    case WM_PAINT:
        {
            PAINTSTRUCT ps;
            HDC hdc = BeginPaint(hWnd, &ps);
            // TODO: 여기에 hdc를 사용하는 그리기 코드를 추가합니다.
			// 해상도 테스트 
			TCHAR temp[100];
			wsprintf(temp, TEXT("해상도 : %d X %d"), SV.GetResolution()[0], SV.GetResolution()[1]);
			TextOut(hdc, 10, 10, temp, lstrlen(temp));
            EndPaint(hWnd, &ps);
        }
        break;
    case WM_DESTROY:
        PostQuitMessage(0);
        break;
    default:
        return DefWindowProc(hWnd, message, wParam, lParam);
    }
    return 0;
}

// 정보 대화 상자의 메시지 처리기입니다.
INT_PTR CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
    UNREFERENCED_PARAMETER(lParam);
    switch (message)
    {
    case WM_INITDIALOG:
        return (INT_PTR)TRUE;

    case WM_COMMAND:
        if (LOWORD(wParam) == IDOK || LOWORD(wParam) == IDCANCEL)
        {
            EndDialog(hDlg, LOWORD(wParam));
            return (INT_PTR)TRUE;
        }
        break;
    }
    return (INT_PTR)FALSE;
}

