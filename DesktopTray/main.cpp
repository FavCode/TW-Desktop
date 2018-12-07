#include <windows.h>

int main(int argc, char *arg[]) {
	if (argc < 2)
		return 1;
	int handle = atoi(arg[1]);
	while (true) {
		if (!IsWindow((HWND)handle)) {
			ShowWindow(FindWindow("Shell_TrayWnd", NULL), SW_SHOW);
			return 0;
		}
		Sleep(1000);
	}
}