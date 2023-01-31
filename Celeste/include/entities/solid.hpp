#pragma once

#include "entities/entity.hpp"

namespace celeste
{
    class Solid : public Entity
    {
    public:
        Solid();

        void Draw() override;
    };
}
