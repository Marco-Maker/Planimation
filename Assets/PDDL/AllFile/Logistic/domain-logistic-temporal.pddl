(define (domain domain-logistic-temporal)
  (:requirements :strips :typing :durative-actions :fluents)
  (:types
    truck airplane - vehicle
    package vehicle - physobj
    airport location - place
    place city - reachable
  )

  (:predicates
    (in-city ?loc - place ?city - city)
    (at ?obj - physobj ?loc - place)
    (in ?pkg - package ?veh - vehicle)
    (link ?a - city ?b - city)
    (is-petrol-station ?l - location)
  )

  (:functions
    (distance ?a - city ?b - city)
    (travel-time ?from - reachable ?to - reachable)
    (flight-time ?from - airport ?to - airport)
    (distance-run ?t - truck)
    (max-distance ?t - truck)
  )

  (:action load-truck
    :parameters (?pkg - package ?truck - truck ?loc - place)
    :precondition (and
      (at ?truck ?loc)
      (at ?pkg ?loc)
    )
    :effect (and
      (not (at ?pkg ?loc))
      (in ?pkg ?truck)
    )
  )

  (:action load-airplane
    :parameters (?pkg - package ?airplane - airplane ?loc - place)
    :precondition (and
      (at ?pkg ?loc)
      (at ?airplane ?loc)
    )
    :effect (and
      (not (at ?pkg ?loc))
      (in ?pkg ?airplane)
    )
  )

  (:action unload-truck
    :parameters (?pkg - package ?truck - truck ?loc - place)
    :precondition (and
      (at ?truck ?loc)
      (in ?pkg ?truck)
    )
    :effect (and
      (not (in ?pkg ?truck))
      (at ?pkg ?loc)
    )
  )

  (:action unload-airplane
    :parameters (?pkg - package ?airplane - airplane ?loc - place)
    :precondition (and
      (in ?pkg ?airplane)
      (at ?airplane ?loc)
    )
    :effect (and
      (not (in ?pkg ?airplane))
      (at ?pkg ?loc)
    )
  )

  (:durative-action drive-truck
    :parameters (?truck - truck ?from - place ?to - place ?city - city)
    :duration (= ?duration (travel-time ?from ?to))
    :condition (and
      (at start (at ?truck ?from))
      (over all (in-city ?from ?city))
      (over all (in-city ?to ?city))
    )
    :effect (and
      (at start (not (at ?truck ?from)))
      (at end   (at ?truck ?to))
    )
  )

  (:durative-action drive-between-cities
    :parameters (?truck - truck ?fromPlace - place ?toPlace - place ?fromCity - city ?toCity - city)
    :duration (= ?duration (travel-time ?fromCity ?toCity))
    :condition (and
      (at start (at ?truck ?fromPlace))
      (over all (in-city ?fromPlace ?fromCity))
      (over all (in-city ?toPlace ?toCity))
      (over all (link ?fromCity ?toCity))
      (at start (<= (+ (distance-run ?truck) (distance ?fromCity ?toCity))
                    (max-distance ?truck)))
    )
    :effect (and
      (at start (not (at ?truck ?fromPlace)))
      (at end   (at ?truck ?toPlace))
      (at end   (increase (distance-run ?truck) (distance ?fromCity ?toCity)))
    )
  )

  (:durative-action refueling
    :parameters (?truck - truck ?station - location)
    :duration (= ?duration 3)
    :condition (and
      (at start (at ?truck ?station))
      (at start (is-petrol-station ?station))
    )
    :effect (at end (assign (distance-run ?truck) 0))
  )

  (:durative-action fly-airplane
    :parameters (?airplane - airplane ?from - airport ?to - airport)
    :duration (= ?duration (flight-time ?from ?to))
    :condition (at start (at ?airplane ?from))
    :effect (and
      (at start (not (at ?airplane ?from)))
      (at end   (at ?airplane ?to))
    )
  )
)
