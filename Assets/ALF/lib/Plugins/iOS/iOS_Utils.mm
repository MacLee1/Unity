
#if !(__IPHONE_OS_VERSION_MIN_REQUIRED < __IPHONE_9_0)

#import <UIKit/UIKit.h>

extern "C" {
    const char * GetBundleVersion();
}

const char * GetBundleVersion()
{
    NSString* pCFBundleVersion = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleVersion"];
    const char *s = [pCFBundleVersion UTF8String];
    char *r = (char *)malloc(strlen(s) + 1);
    strcpy(r, s);
    return r;
}


#endif // !(__IPHONE_OS_VERSION_MIN_REQUIRED < __IPHONE_9_0)
