;; logistics domain Typed version.
;;

(define (domain logistics)
  (:requirements :strips :typing)
  (:types
    truck airplane - vehicle
    package vehicle - physobj
    airport location - place
    city place physobj - object
  )

  (:predicates
    (in-city ?loc - place ?city - city) ;If a place (airport or location) is in a city
    (at ?obj - physobj ?loc - place) ;true if a phisical object (package, truck, airplane) is in a place (airport or location)
    (in ?pkg - package ?veh - vehicle) ;true if a package is in a vehicle (truck or location)
  )

  ; A truck can be loaded if both the truck and the package are at the location
  ; the result is that the package is no longer in the location but inside the truck
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

  ; A plane can be loaded if both the plane and the package are at the location
  ; the result is that the package is no longer in the airport but inside the plane
  (:action load-airplane
    :parameters (?pkg - package ?airplane - airplane ?loc - airport)
    :precondition (and
      (at ?pkg ?loc)
      (at ?airplane ?loc)
    )
    :effect (and
      (not (at ?pkg ?loc))
      (in ?pkg ?airplane)
    )
  )

  ; Unloading the truck if the package is inside the truck
  ; The result is that the package is at the place of the truck and no longer in the truck
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

  ; Unloading the plane if the package is inside the plane
  ; The result is that the package is at the airport and no longer in the plane
  (:action unload-airplane
    :parameters (?pkg - package ?airplane - airplane ?loc - airport)
    :precondition (and
      (in ?pkg ?airplane)
      (at ?airplane ?loc)
    )
    :effect (and
      (not (in ?pkg ?airplane))
      (at ?pkg ?loc)
    )
  )

  ; A truck can always drive within a city 
  (:action drive-truck
    :parameters (?truck - truck ?loc-from - place ?loc-to - place ?city - city)
    :precondition (and
      (at ?truck ?loc-from)
      (in-city ?loc-from ?city)
      (in-city ?loc-to ?city)
    )
    :effect (and
      (not (at ?truck ?loc-from))
      (at ?truck ?loc-to)
    )
  )

  ; A plane can only fly between cities which have an airport
  ; so ?loc-from and loc-to are airpors, not places
  (:action fly-airplane
    :parameters (?airplane - airplane ?loc-from - airport ?loc-to - airport)
    :precondition (at ?airplane ?loc-from)
    :effect (and
      (not (at ?airplane ?loc-from))
      (at ?airplane ?loc-to)
    )
  )
)