#include "colliders/circle.hpp"

#include "globals.hpp"
#include "imgui_utils.hpp"
#include "colliders/hitbox.hpp"

namespace colliders
{
    Circle::Circle(const calc::Vector2 &position, const float& radius)
        : Collider(position), mRadius(radius)
    {
        mType = ColliderType::Circle;
    }

    void Circle::Render(ImColor color) const
    {
        celeste::Globals::gDrawList->AddCircle(ToImVec2(mPosition), mRadius, color);
    }

    bool Circle::Collide(const calc::Vector2& point) const
    {
        return (point - mPosition).GetNorm() <= mRadius;
    }

    bool Circle::Collide(const Hitbox& hitbox) const
    {
        // Implementation is in 'colliders/hitbox.cpp'
        return hitbox.Collide(*this);
    }

    bool Circle::Collide(const Circle& circle) const
    {
        return calc::Dist(mPosition, circle.mPosition) < mRadius + circle.mRadius;
    }
}