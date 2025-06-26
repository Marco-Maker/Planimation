;; The trucks now have a limited amount of distance they can run
;;
;; drive (both within and between cities), fly, and refueling actions are now durative 

(define (domain logistics)
  (:requirements :strips :typing)
  (:types
    truck airplane - vehicle
    package vehicle - physobj
    airport location - place
    place city - reachable
  )

  (:predicates
    (in-city ?loc - place ?city - city) ;If a place (airport or location) is in a city
    (at ?obj - physobj ?loc - place) ;true if a phisical object (package, truck, airplane) is in a place (airport or location)
    (in ?pkg - package ?veh - vehicle) ;true if a package is in a vehicle (truck or location)
    (link ?a - city ?b - city) ;true if two cities are connected
    (is-petrol-station ?l - location) ;true if the location l is a petrol station
  )

  (:functions
    (distance ?a - city ?b - city) ; distance between cities a? and b?
    (travel-time ?from - reachable ?to - reachable) ; travel time ?from -> ?to
    (flight-time ?from - airport ?to - airport); flight time ?from -> ?to
    (distance-run ?t - truck) ; the distance ?t can run without refueling
    (max-distance ?t - truck) ; maximum distance t? can travel without refueling
  )

  ; A truck can be loaded if both the truck and the packages is at the location
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

  ; A plance can be loaded if both the plance and the packages is at the location
  ; the result is that the package is no longer in the location but inside the plance
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
  ; The result is that the package is at the place of the plane and no longer in the plane
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

  ; A truck can always drive between locations inside cities
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
      (at end (at ?truck ?to))
    )
  )
 
  ; A truck can drive between places of different cities if connected
  (:durative-action drive-between-cities
    :parameters (?truck - truck ?fromPlace - place ?toPlace - place ?fromCity - city ?toCity - city)
    :duration (= ?duration (travel-time ?fromCity ?toCity))
    :condition (and
      (at start (at ?truck ?fromPlace))
      (over all (in-city ?fromPlace ?fromCity))
      (over all (in-city ?toPlace ?toCity))
      (over all (link ?fromCity ?toCity))
      (at start (<= (+ (distance-run ?truck) (distance ?fromCity ?toCity)) (max-distance ?truck)))
    )
    :effect (and
      (at start (not (at ?truck ?fromPlace)))
      (at end (at ?truck ?toPlace))
      (at end (increase
          (distance-run ?truck)
          (distance ?fromCity ?toCity)))
    )
  )

  ; This action allows refueling at petrol station
  (:durative-action refueling
    :parameters (?truck - truck ?station - location)
    :duration (= ?duration 3)
    :condition (and
      (at start (at ?truck ?station))
      (at start (is-petrol-station ?station))
    )
    :effect (and
      (at end (assign (distance-run ?truck) 0))  ; distance-run sets to 0 so the truck can drive again
    )
  )
  
  ; A plane can only fly between cities which have an airport
  (:durative-action fly-airplane
    :parameters (?airplane - airplane ?from - airport ?to - airport)
    :duration (= ?duration (flight-time ?from ?to))
    :condition (and
      (at start (at ?airplane ?from)))
    :effect (and
      (at start (not (at ?airplane ?from)))
      (at end (at ?airplane ?to))
    )
  )
)




