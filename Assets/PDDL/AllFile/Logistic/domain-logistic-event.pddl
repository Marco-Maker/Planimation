(define (domain logistics-pddlplus)
  (:requirements :strips :typing :fluents :durative-actions :processes :events)
  (:types
    truck airplane - vehicle
    package vehicle - physobj
    airport location - place
    city place physobj - object
  )

  (:predicates
    ;; Stati standard
    (at ?obj - physobj ?loc - place)
    (in ?pkg - package ?veh - vehicle)
    (en-route ?v - vehicle ?from - place ?to - place)

    ;; Nuovi predicati di connessione territoriale
    (in-city   ?loc  - place ?city  - city)  ; ?loc appartiene a ?city
    (link      ?c1   - city  ?c2    - city)  ; c’è un collegamento (stradale/aereo) fra due città
  )

  (:functions
    (distance-left ?v - vehicle)
    (speed         ?v - vehicle)
    (distance      ?from - place ?to - place)
  )

  ;; Caricamento / scaricamento
  (:action load-truck
    :parameters (?pkg - package ?truck - truck ?loc - place)
    :precondition (and (at   ?truck ?loc)
                       (at   ?pkg   ?loc))
    :effect       (and (not  (at   ?pkg   ?loc))
                       (in   ?pkg   ?truck))
  )

  (:action unload-truck
    :parameters (?pkg - package ?truck - truck ?loc - place)
    :precondition (and (at   ?truck ?loc)
                       (in   ?pkg   ?truck))
    :effect       (and (not  (in   ?pkg   ?truck))
                       (at   ?pkg   ?loc))
  )

  (:action load-airplane
    :parameters (?pkg - package ?plane - airplane ?loc - airport)
    :precondition (and (at   ?plane ?loc)
                       (at   ?pkg   ?loc))
    :effect       (and (not  (at   ?pkg   ?loc))
                       (in   ?pkg   ?plane))
  )

  (:action unload-airplane
    :parameters (?pkg - package ?plane - airplane ?loc - airport)
    :precondition (and (at   ?plane ?loc)
                       (in   ?pkg  ?plane))
    :effect       (and (not  (in   ?pkg  ?plane))
                       (at   ?pkg  ?loc))
  )

  ;; Azioni di movimento con processi continui
  (:action start-drive
    :parameters (?truck - truck ?from - location ?to - location ?city - city)
    :precondition (and
      (at        ?truck ?from)
      (in-city   ?from  ?city)
      (in-city   ?to    ?city)
    )
    :effect (and
      (not       (at        ?truck ?from))
      (en-route  ?truck ?from ?to)
      (assign    (distance-left ?truck) (distance ?from ?to))
      (assign    (speed         ?truck) 10)
    )
  )

  (:process driving
    :parameters (?truck - truck ?from - place ?to - place)
    :precondition (en-route ?truck ?from ?to)
    :effect (decrease (distance-left ?truck) (* #t (speed ?truck)))
  )

  (:event truck-arrives
    :parameters (?truck - truck ?from - place ?to - place)
    :precondition (and (en-route ?truck ?from ?to)
                       (<=      (distance-left ?truck) 0))
    :effect (and (not (en-route ?truck ?from ?to))
                 (at  ?truck ?to))
  )

  ;; Spostamento tra città diverse (su strada)
  (:action start-drive-between-cities
    :parameters (?truck     - truck
                 ?fromPlace - location
                 ?toPlace   - location
                 ?c1        - city
                 ?c2        - city)
    :precondition (and
      (at        ?truck     ?fromPlace)
      (in-city   ?fromPlace ?c1)
      (in-city   ?toPlace   ?c2)
      (link      ?c1        ?c2)
    )
    :effect (and
      (not       (at        ?truck ?fromPlace))
      (en-route  ?truck     ?fromPlace ?toPlace)
      (assign    (distance-left ?truck) (distance ?fromPlace ?toPlace))
      (assign    (speed         ?truck) 10)
    )
  )

  ;; Volo aereo
  (:action start-fly
    :parameters (?plane - airplane ?from - airport ?to - airport ?c1 - city ?c2 - city)
    :precondition (and
      (at        ?plane ?from)
      (in-city   ?from  ?c1)
      (in-city   ?to    ?c2)
      (link      ?c1    ?c2)    ; es. rotta aerea disponibile
    )
    :effect (and
      (not       (at        ?plane ?from))
      (en-route  ?plane ?from ?to)
      (assign    (distance-left ?plane) (distance ?from ?to))
      (assign    (speed         ?plane) 250)
    )
  )

  (:process flying
    :parameters (?plane - airplane ?from - airport ?to - airport)
    :precondition (en-route ?plane ?from ?to)
    :effect (decrease (distance-left ?plane) (* #t (speed ?plane)))
  )

  (:event airplane-arrives
    :parameters (?plane - airplane ?from - airport ?to - airport)
    :precondition (and (en-route ?plane ?from ?to)
                       (<=      (distance-left ?plane) 0))
    :effect (and (not (en-route ?plane ?from ?to))
                 (at  ?plane ?to))
  )
)
