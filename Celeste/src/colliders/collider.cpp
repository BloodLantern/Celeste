#include "colliders/collider.hpp"

#include <cassert>

#include "colliders/hitbox.hpp"
#include "colliders/circle.hpp"

namespace colliders
{
    Collider::Collider(const calc::Vector2 &position)
        : mPosition(position)
    {
    }

    bool Collider::Collide(const Collider& collider) const
    {
        assert(collider.mType != ColliderType::None && "Invalid collider type in collision check.");

        switch(collider.mType)
        {
            case ColliderType::Hitbox:
                return Collide(dynamic_cast<const Hitbox&>(collider));
            case ColliderType::Circle:
                return Collide(dynamic_cast<const Circle&>(collider));
            default:
                throw std::runtime_error("Invalid collider type in collision check.");
        }
    }
}
