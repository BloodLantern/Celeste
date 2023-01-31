#pragma once

#include <calc/vector2.hpp>

namespace celeste
{
    struct InputData
    {
        bool active = false;
    };

    struct DirectionInputData : public InputData
    {
        calc::Vector2 vec;
        bool moving = false;
    };

    class Input
    {
    public:
        DirectionInputData direction;
        InputData jump;

        void Update();
    };
}
