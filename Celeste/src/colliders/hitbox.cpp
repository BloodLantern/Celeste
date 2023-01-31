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
        calc::Vector2 position = hitbox.GetPosition(), size = hitbox.GetSize();

        // Check for a collision with any of the hitbox's corners
        return Collide(position) // Top left
            || Collide(calc::Vector2(position.x + size.x, position.y)) // Top right
            || Collide(calc::Vector2(position.x, position.y + size.y)) // Bottom left
            || Collide(calc::Vector2(position.x + size.x, position.y + size.y)); // Bottom right
    }

    bool Hitbox::Collide(const Circle& circle) const
    {
        // TODO: Implement Hitbox-Circle collisions
        /*calc::Vector2 oc = circle.GetPosition();

        calc::Vector2 os2[] = {
            createVector2(box->rectangle.points[0].x, box->rectangle.points[0].y),
            createVector2(box->rectangle.points[1].x, box->rectangle.points[1].y),
            createVector2(box->rectangle.points[2].x, box->rectangle.points[2].y),
            createVector2(box->rectangle.points[3].x, box->rectangle.points[3].y)
        };

        for (int i = 0; i < 4; i++) {
            calc::Vector2 normal = getNormalVector2FromSegment(box->triangle.points[i], box->triangle.points[(i + 1) % 4]);
            
            calc::Vector2 temp = multiplyVector2(normal, circle.mRadius);
            calc::Vector2 od1 = addVector2(oc, oppositeVector2(temp));
            calc::Vector2 od2 = addVector2(oc, temp);
            float sProjections[2];
            sProjections[0] = dotProductVector2(normal, od1);
            sProjections[1] = dotProductVector2(normal, od2);

            float s2Projections[4];
            for (int j = 0; j < 4; j++) {
                s2Projections[j] = dotProductVector2(normal, os2[j]);
            }

            float max = getMax(sProjections, 2);
            float min = getMin(sProjections, 2);
            float max2 = getMax(s2Projections, 4);
            float min2 = getMin(s2Projections, 4);

            if (__max(min, min2) > __min(max, max2))
                return false;
        }*/

        return true;
    }
}