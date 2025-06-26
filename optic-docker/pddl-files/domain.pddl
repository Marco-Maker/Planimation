;; domain.pddl
(define (domain logistics_simple)
  (:requirements :typing :durative-actions :numeric-fluents)
  (:types
    truck - object
    place city - object
  )
  (:predicates
    (at       ?t - truck ?p - place)
    (in-city  ?p - place ?c - city)
    (link     ?c1 - city  ?c2 - city)
  )
  (:functions
    (travel-time   ?from - place ?to - place)
    (distance-run  ?t    - truck)
    (max-distance  ?t    - truck)
  )

  (:durative-action drive-truck
    :parameters (?t - truck ?from - place ?to - place ?c1 - city ?c2 - city)
    :duration (= ?duration (travel-time ?from ?to))
    :condition (and
      (at start    (at ?t ?from))
      (at start    (<= (+ (distance-run ?t)
                          (travel-time ?from ?to))
                       (max-distance ?t)))
      (over all   (in-city ?from ?c1))
      (over all   (in-city ?to   ?c2))
      (over all   (link ?c1 ?c2))
    )
    :effect (and
      (at start    (not (at ?t ?from)))
      (at end      (at ?t ?to))
      (at end      (increase (distance-run ?t)
                             (travel-time ?from ?to)))
    )
  )
)
