using SDL;

namespace battlesdk.graphics;
public static class CustomBlendModes {
    public static SDL_BlendMode Subtract = 0;
    public static SDL_BlendMode Add = 0;

    public static void Init () {
        Subtract = SDL3.SDL_ComposeCustomBlendMode(
            SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
            SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
            SDL_BlendOperation.SDL_BLENDOPERATION_REV_SUBTRACT,
            SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
            SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
            SDL_BlendOperation.SDL_BLENDOPERATION_REV_SUBTRACT
        );
        Add = SDL3.SDL_ComposeCustomBlendMode(
            SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
            SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
            SDL_BlendOperation.SDL_BLENDOPERATION_ADD,
            SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
            SDL_BlendFactor.SDL_BLENDFACTOR_ONE,
            SDL_BlendOperation.SDL_BLENDOPERATION_ADD
        );
    }
}
