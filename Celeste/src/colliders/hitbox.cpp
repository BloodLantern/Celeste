#include "colliders/hitbox.hpp"

#include "globals.hpp"
#include "imgui_utils.hpp"
#include "colliders/circle.hpp"

namespace colliders
{
    Hitbox::Hitbox(const calc::Vector2 &position, const calc::Vector2& size)
        : Collider(position), mSize(size)
    {
        mType = ColliderType::Hitbox;
    }

    void Hitbox::Render(ImColor color) const
    {
        celeste::Globals::gDrawList->AddRect(ToImVec2(mPosition), ToImVec2(mSize), color);
    }

    bool Hitbox::Collide(const calc::Vector2& point) const
    {
        return point.x < GetRight() && point.x > GetLeft()
            && point.y < GetBottom() && point.y > GetTop();
    }

    bool Hitbox::Collide(const Hitbox& hitbox) const
    {
        calc::Vector2 position = hitbox.mPosition, size = hitbox.mSize;

        // Check for a collision with any of the hitbox's corners
        return Collide(position) // Top left
            || Collide(calc::Vector2(position.x + size.x, position.y)) // Top right
            || Collide(calc::Vector2(position.x, position.y + size.y)) // Bottom left
            || Collide(calc::Vector2(position.x + size.x, position.y + size.y)); // Bottom right
    }

    bool Hitbox::Collide(const Circle& circle) const
    {
        if (Collide(circle.GetPosition()))
            return true;

        const calc::Vector2& topLeft = mPosition;
        const calc::Vector2 topRight(mPosition.x + mSize.x, mPosition.y),
            bottomLeft(mPosition.x, mPosition.y + mSize.y),
            bottomRight(mPosition.x + mSize.x, mPosition.y + mSize.y);
            
        return circle.Intersect(topLeft, topRight)
            || circle.Intersect(topRight, bottomRight)
            || circle.Intersect(bottomRight, bottomLeft)
            || circle.Intersect(bottomLeft, topLeft);
    }
}