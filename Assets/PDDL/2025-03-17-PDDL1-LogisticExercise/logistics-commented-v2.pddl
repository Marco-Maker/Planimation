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
    (in ?pkg - package ?veh - vehicle) ;true if a package is in a vehicle (truck or airplane)
  )

   ; Given that both trucks and airplane are vehichles, we can use a single load and unload actions
   ; (this would allow packages to be taken airport also in place that are not airport, but this instantion
   ; is useless given that then airplane can only fly between airport and are only in places that are airports)
   ; 
  ; A vehicle can be loaded if both the vehicle and the packages is at the location
  ; the result is that the package is no longer in the location but inside the vehicle
  (:action load-vehicle
    :parameters (?pkg - package ?vehicle - vehicle ?loc - place)
    :precondition (and
      (at ?vehicle ?loc)
      (at ?pkg ?loc)
    )
    :effect (and
      (not (at ?pkg ?loc))
      (in ?pkg ?vehicle)
    )
  )

  ; Unloading the vehicle if the package is inside the vehicle
  ; The result is that the package is at the place of the vehicle and no longer in the vehicle
  (:action unload-vehicle
    :parameters (?pkg - package ?vehicle - vehicle ?loc - place)
    :precondition (and
      (at ?vehicle ?loc)
      (in ?pkg ?vehicle)
    )
    :effect (and
      (not (in ?pkg ?vehicle))
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
  (:action fly-airplane
    :parameters (?airplane - airplane ?loc-from - airport ?loc-to - airport)
    :precondition (at ?airplane ?loc-from)
    :effect (and
      (not (at ?airplane ?loc-from))
      (at ?airplane ?loc-to)
    )
  )
)